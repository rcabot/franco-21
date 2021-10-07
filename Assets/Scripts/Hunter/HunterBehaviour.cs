using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

[RequireComponent(typeof(AudioSource), typeof(Rigidbody))]
public class HunterBehaviour : MonoBehaviour
{
    //Members
    private float                              m_PlayerAggro = 0f;
    private float                              m_TimeUntilPeriodEffect = 0f;
    private AudioSource                        m_AudioSource;
    private Rigidbody                          m_RigidBody;
    private SphereCollider                     m_BiteTriggerSphere;
    private SkinnedMeshRenderer                m_MeshRenderer;
    private HunterStateSettings                m_CurrentStateSettings;
    private HunterState                        m_CurrentState;
    private SubmarineController                m_PlayerSubmarine;
    private Animator                           m_Animator;
    private Coroutine                          m_RunningBehaviour;
    private static readonly int                m_AttackTriggerID = Animator.StringToHash("AttackTrigger");

    private Vector3                            m_Velocity;
    [SerializeField] private Vector3           m_MoveTarget;

    [Header("Physics")]
    [SerializeField] private float             m_Acceleration = 2f;
    [SerializeField] private float             m_TurnSpeed = 4f;
    [SerializeField] private float             m_BiteKnockbackForce = 40f;
    [SerializeField] private AnimationCurve    m_FrictionCurve = new AnimationCurve();

    [Header("Behaviour")]
    [SerializeField] private int               m_MaxPlayerAggro = 100;
    [SerializeField] private Vector3           m_BackstagePosition = new Vector3(0f, 0f, -10000f);

    [SerializeField, Range(1f, 100f), Tooltip("Distance that it's safe for the creature to appear/disappear at even if infront of the player camera")]
    private float                              m_SafeVisibilityDistance = 10f;
    
    //Custom inspector would be nice here. I'm restraining myself by not writing one
    [SerializeField] private HunterStateSettings m_BackstageSettings            = new HunterStateSettings();
    [SerializeField] private HunterStateSettings m_FrontstageIdleSettings       = new HunterStateSettings();
    [SerializeField] private HunterStateSettings m_FrontstageDistantSettings    = new HunterStateSettings();
    [SerializeField] private HunterStateSettings m_FrontstageSuspiciousSettings = new HunterStateSettings();
    [SerializeField] private HunterStateSettings m_FrontstageCloseSettings      = new HunterStateSettings();
    [SerializeField] private HunterStateSettings m_AttackingSettings            = new HunterStateSettings();
    [SerializeField] private HunterStateSettings m_RetreatSettings              = new HunterStateSettings();

    [SerializeField] private HunterAggroSettings m_CreatureAggroSettings                = new HunterAggroSettings();


    //Populated in awake. /!\ REMEMBER TO UPDATE THIS IF YOU ADD A NEW STATE /!\ 
    List<HunterStateSettings>                    m_AllStateSettings = new List<HunterStateSettings>();

    //Events
    public event EventHandler<float>                                          OnAggroChanged;
    public event EventHandler<KeyValuePair<HunterState, HunterStateSettings>> OnStateChanged;

    //Properties
    public static HunterBehaviour Instance { get; private set; }
    public float PlayerAggro
    {
        get => m_PlayerAggro;
        set
        {
            //No Aggro changes in retreat
            if (m_CurrentState != HunterState.Retreat)
            {
                float clamped_value = Mathf.Clamp(value, 0f, MaxPlayerAggro);
                if (!Mathf.Approximately(m_PlayerAggro, clamped_value))
                {
                    m_PlayerAggro = clamped_value; HandleAggroChanged();
                }
            }
        }
    }

    public int MaxPlayerAggro => m_MaxPlayerAggro;

    public HunterStateSettings CurrentStateSettings => m_CurrentStateSettings;
    public HunterState         CurrentState => m_CurrentState;

    public bool                AttackEnabled { get; set; }
    public HunterAggroSettings Aggro => m_CreatureAggroSettings;
    public bool                InLimbo => !m_MeshRenderer.enabled;

    //Methods
    public void ForceSetState(HunterState state)
    {
        LogHunterMessage($"[Hunter] Forced state change: {state}");
        EnterState(state);
    }

    private void HandleAggroChanged()
    {
        OnAggroChanged?.Invoke(this, m_PlayerAggro);

        //If the creature is attacking or retreating then aggro changes are ignored
        switch (CurrentState)
        {
            case HunterState.Attacking:
            case HunterState.Retreat:
                return;
        }

        float TriggerAggroForStateIfValid(HunterStateSettings state_settings)
        {
            return state_settings.ActivationAggro <= m_PlayerAggro ? state_settings.ActivationAggro : float.NegativeInfinity;
        }

        //Test if a new state should be activated
        HunterStateSettings best_state_settings = m_AllStateSettings.Aggregate((a, b) => TriggerAggroForStateIfValid(a) < TriggerAggroForStateIfValid(b) ? b : a);
        HunterState new_state = (HunterState)m_AllStateSettings.IndexOf(best_state_settings);

        if (new_state != m_CurrentState)
        {
            EnterState(new_state);
        }
    }

    private void EnterState(HunterState state)
    {
        LogHunterMessage($"[Hunter] Entering State:  {state}");

        m_CurrentState = state;
        m_CurrentStateSettings = m_AllStateSettings[(int)state];
        StartStateBehaviour();

        //Apply enter effects
        if (m_CurrentStateSettings != null)
        {
            PlayStateEnterSound();
            ApplyScreenShake(m_CurrentStateSettings.StartScreenShakeMagnitude, m_CurrentStateSettings.StartScreenShakeDuration);

            //Setup the period effects
            m_TimeUntilPeriodEffect = m_CurrentStateSettings.PeriodTimeRange.RandomValue;
        }

        OnStateChanged?.Invoke(this, new KeyValuePair<HunterState, HunterStateSettings>(m_CurrentState, m_CurrentStateSettings));
    }

    private void UpdatePeriodicEffects()
    {
        if (m_TimeUntilPeriodEffect > 0f)
        {
            m_TimeUntilPeriodEffect -= Time.deltaTime;

            if (m_TimeUntilPeriodEffect <= 0f)
            {
                if (m_CurrentStateSettings != null)
                {
                    m_TimeUntilPeriodEffect = m_CurrentStateSettings.PeriodTimeRange.RandomValue;

                    PlayPeriodicSound();
                    ApplyScreenShake(m_CurrentStateSettings.PeriodicScreenShakeMagnitude, m_CurrentStateSettings.PeriodicScreenShakeDuration);

                    LogHunterMessage($"[Hunter] Periodic Effects Triggered. Time Until Next: {m_TimeUntilPeriodEffect: #.##} seconds");
                }
                else
                {
                    m_TimeUntilPeriodEffect = 0f;
                }
            }
        }
    }

    private void PlayStateEnterSound()
    {
        if (m_CurrentStateSettings != null)
        {
            m_AudioSource.clip = m_CurrentStateSettings.StateStartSounds?.RandomSound;
            if (m_AudioSource.clip)
            {
                m_AudioSource.volume = m_CurrentStateSettings.StartSoundVolume;
                m_AudioSource.Play();
            }
        }
    }

    private void PlayPeriodicSound()
    {
        if (m_CurrentStateSettings != null)
        {
            m_AudioSource.clip = m_CurrentStateSettings.PeriodicSounds?.RandomSound;
            if (m_AudioSource.clip)
            {
                m_AudioSource.volume = m_CurrentStateSettings.PeriodicSoundVolumeScale;
                m_AudioSource.Play();
            }
        }
    }

    private void ApplyScreenShake(float magnitude, float duration)
    {
        if (duration > 0f && !Mathf.Approximately(magnitude, 0f))
        {
            WorldShakeManager.Instance?.Shake(magnitude, duration);
        }
    }

    private void PlayerBitten(SubmarineController player)
    {
        LogHunterMessage("[Hunter] Player has been hit");

        //Hit the player and knock them back
        Vector3 force_dir = transform.forward;
        force_dir.y = 0f; //Don't knock the player up/down
        player.AddImpulse(force_dir.normalized * m_BiteKnockbackForce);
        player.TakeHit();

        //Start retreating
        PlayerAggro = 0f;
        EnterState(HunterState.Retreat);
    }

    private bool FindPositionInPlayerRange(float distance, out Vector3 result)
    {
        const int max_tries = 20;
        const float half_pi = Mathf.PI * 0.5f;

        result = Vector3.zero;

        TerrainManager terrain_manager = TerrainManager.Instance;
        Rect playable_area = terrain_manager.PlayableTerrainArea;
        Vector3 player_position = m_PlayerSubmarine.transform.position;
        Vector2 player_position_2d = player_position.XZ();
        Vector2 base_direction = Vector2.up;

        float min_angle = 0f;
        float max_angle = Mathf.PI * 2;

        //We're spwaning close to the player so only spawn in a wedge to their back direction (on the XZ plane)
        if (distance < m_SafeVisibilityDistance)
        {
            base_direction = -m_PlayerSubmarine.submarineCockpit.forward.XZ().normalized;
            min_angle = -half_pi;
            max_angle = half_pi;
        }

        bool GetSpawnPointOnTerrain(Vector2 position_2d, out Vector3 out_position)
        {
            if (playable_area.Contains(position_2d))
            {
                float potential_location_height = Mathf.Max(player_position.y + m_CurrentStateSettings.PlayerHeightOffset.RandomValue, terrain_manager.GetTerrainElevation(position_2d.XZ()));
                

                //Height distance can't be greater than the required distance
                if (Math.Abs(player_position.y - potential_location_height) < distance)
                {
                    out_position = position_2d.XZ() + Vector3.up * potential_location_height;
                    return true;
                }

            }

            out_position = Vector3.zero;
            return false;
        }

        //Attempt to position randomly around the player
        for (int i = 0; i < max_tries; ++i)
        {
            Vector2 direction = base_direction.RotateCw(UnityRandom.Range(min_angle, max_angle));
            Vector2 potential_spawn_location = player_position_2d + direction * distance;

            if (GetSpawnPointOnTerrain(potential_spawn_location, out result))
            {
                LogHunterMessage($"[Hunter] Spawn Position Found: {result}. | Distance: {(result - player_position).magnitude} | Angle: {Vector2.Angle(m_PlayerSubmarine.submarineCockpit.forward.XZ().normalized, (result - player_position).XZ().normalized)}");
                return true;
            }
        }

        //Last ditch attempt to go straight behind the player
        if (GetSpawnPointOnTerrain(player_position_2d - base_direction * distance, out result))
        {
            LogHunterMessage($"[Hunter] Spawn Position Found: {result}. | Distance: {(result - player_position).magnitude} | Angle: {Vector2.Angle(m_PlayerSubmarine.submarineCockpit.forward.XZ().normalized, (result - player_position).XZ().normalized)}");
            return true;
        }

        LogHunterMessage("[Hunter] Failed to find spawn position");
        return false;
    }

    private void MoveTowardsTarget()
    {
        Vector3 position = m_RigidBody.position;
        Vector3 target = m_MoveTarget != Vector3.zero ? m_MoveTarget : position;

        Vector3 target_direction = target - position;
        float target_distance = target_direction.magnitude;

        if (!Mathf.Approximately(0f, target_distance))
        {
            target_direction /= target_distance; //normalise the direction
            m_Velocity += m_Acceleration * target_direction * Time.fixedDeltaTime;
        }
        else
        {
            target_direction = transform.forward;
        }

        // apply friction
        float current_speed = m_Velocity.magnitude;
        Vector3 velocity_direction = Vector3.zero;

        if (current_speed > 0f)
        {
            velocity_direction = m_Velocity / current_speed;

            float friction = m_FrictionCurve.Evaluate(current_speed);
            m_Velocity = velocity_direction * Mathf.Max(0f, current_speed - friction);
        }

        //Apply move
        m_RigidBody.MovePosition(position + m_Velocity);

        //Rotate towards velocity
        Quaternion target_rotation = m_RigidBody.rotation;
        if (current_speed > 0f)
        {
            target_rotation = Quaternion.LookRotation(velocity_direction, Vector3.up);
        }
        else if (m_MoveTarget  != Vector3.zero)
        {
            target_rotation = Quaternion.LookRotation(target_direction, Vector3.up);
        }

        m_RigidBody.MoveRotation(Quaternion.RotateTowards(m_RigidBody.rotation, target_rotation, m_TurnSpeed));
    }

    //Woo coroutines
    private void StartStateBehaviour()
    {
        if (m_RunningBehaviour != null)
        {
            StopCoroutine(m_RunningBehaviour);
        }

        switch (CurrentState)
        {
            case HunterState.Backstage:
                if (!InLimbo)
                {
                    m_RunningBehaviour = StartCoroutine(Retreat(CurrentState));
                }
                break;
            case HunterState.Retreat:
                m_RunningBehaviour = StartCoroutine(Retreat(CurrentState));
                break;
            case HunterState.FrontstageIdle:
            case HunterState.FrontstageDistant:
            case HunterState.Suspicious:
            case HunterState.FrontstageClose:
                m_RunningBehaviour = StartCoroutine(FrontstageAvoid(CurrentState));
                break;
            case HunterState.Attacking:
                m_RunningBehaviour = StartCoroutine(Attacking());
                break;
            default:
                LogHunterMessage("[Hunter] Unknown follow state. Missing code");
                break;
        }
    }

    #region Behaviour Coroutines
    private IEnumerator CoLeaveLimbo(HunterState state, float distance)
    {
        //Go frontstage if not already
        while (InLimbo)
        {
            yield return new WaitForFixedUpdate();

            if (CurrentState != state)
            {
                yield break;
            }

            LogHunterMessage("[Hunter] Trying to leave limbo");
            if (FindPositionInPlayerRange(distance, out Vector3 spawn_position))
            {
                LeaveLimbo(spawn_position);
            }
        }
    }

    private IEnumerator FrontstageAvoid(HunterState state)
    {
        yield return new WaitForFixedUpdate();

        //Go frontstage if not already
        if (InLimbo)
        {
            yield return StartCoroutine(CoLeaveLimbo(state, m_CurrentStateSettings.PlayerDistance));
        }

        float sqr_vis_distance = m_SafeVisibilityDistance * m_SafeVisibilityDistance;
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (CurrentState != state)
            {
                yield break;
            }

            //Idle behaviour per-tick

            //Avoid the player
            Vector3 from_player = m_PlayerSubmarine.transform.position - transform.position;
            float sqr_dist_to_player = from_player.sqrMagnitude;
            if (sqr_dist_to_player < sqr_vis_distance)
            {
                LogHunterMessage("[Hunter] Too close to player. Fleeing");
                m_MoveTarget = transform.position + (from_player / Mathf.Sqrt(sqr_dist_to_player)) * 1000f;

                //Just flee and try and get to a new position
                yield return new WaitForSeconds(1f);
                yield return new WaitForFixedUpdate();

                m_MoveTarget = Vector3.zero;

                if (FindPositionInPlayerRange(m_CurrentStateSettings.PlayerDistance, out Vector3 spawn_position))
                {
                    LeaveLimbo(spawn_position);
                }
            }
        }
    }

    private IEnumerator Attacking()
    {
        //Go frontstage if not already
        if (InLimbo)
        {
            yield return StartCoroutine(CoLeaveLimbo(HunterState.Attacking, m_SafeVisibilityDistance * 2f));
        }

        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (CurrentState != HunterState.Attacking)
            {
                break;
            }

            //Attacking behaviour per-tick

            //Seek the player
            Vector3 position = m_RigidBody.position;

            //We want to line up the bite sphere with the player. So offset the player position to account for it
            Vector3 bite_sphere_position = m_BiteTriggerSphere.bounds.center;
            Vector3 bite_sphere_offset = bite_sphere_position - position;
            Vector3 player_position = m_PlayerSubmarine.transform.position - bite_sphere_offset;
            m_MoveTarget = player_position;

            Vector3 to_attack_position = player_position - position;
            float sqr_distance_to_attack_position = to_attack_position.sqrMagnitude;
            float sqr_player_distance = m_CurrentStateSettings.PlayerDistance * m_CurrentStateSettings.PlayerDistance;

#if UNITY_EDITOR
            LogHunterMessage($"[Hunter] Distance to attack: {Mathf.Sqrt(sqr_distance_to_attack_position):#.##}");
#endif

            if (sqr_distance_to_attack_position < sqr_player_distance && m_CurrentStateSettings.PlayerHeightOffset.InRange(Mathf.Abs(to_attack_position.y)))
            {
                //Trigger an attack
                m_Animator.SetTrigger(m_AttackTriggerID);
            }
            else
            {
                m_Animator.ResetTrigger(m_AttackTriggerID);
            }
        }

        //Make sure attack gets reset
        m_Animator.ResetTrigger(m_AttackTriggerID);
    }

    private IEnumerator Retreat(HunterState state)
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (m_CurrentState != state)
            {
                yield break;
            }

            //Despawn if far enough away from the player
            Vector3 player_pos = m_PlayerSubmarine.transform.position;
            Vector3 to_player = player_pos - transform.position;
            float distance_to_player = to_player.magnitude;

            if (distance_to_player >= m_SafeVisibilityDistance)
            {
                LogHunterMessage("[Hunter] Tring to Enter Limbo - Far enough away to enter limbo");
                GoBackstage();
                yield break;
            }
            else
            {
                LogHunterMessage($"[Hunter] Tring to Enter Limbo - Too close to player to enter limbo. Distance: {distance_to_player: #.##}");
                //Flee from the player
                Vector3 flee_direction = -(to_player / distance_to_player);

                //Y must be positive
                if (flee_direction.y < 0.3f)
                {
                    flee_direction.y = 0.3f;
                    flee_direction.Normalize();
                }

                m_MoveTarget = transform.position + flee_direction * 1000f;
            }
        }
    }

    #endregion

    private void GoBackstage()
    {
        EnterLimbo();
        transform.position = m_BackstagePosition;
        EnterState(HunterState.Backstage);
    }

    private void EnterLimbo()
    {
        m_RigidBody.isKinematic = true;
        m_MeshRenderer.enabled = false;
        m_MoveTarget = Vector3.zero;
    }

    private void LeaveLimbo(Vector3 position)
    {
        //Teleport the creature way above the target position and then move them down with the physics engine.
        const float teleport_drop_height = 1000f;
        const float surface_offset = 0.5f;

        transform.position = position + Vector3.up * teleport_drop_height;
        m_MeshRenderer.enabled = true;
        m_RigidBody.isKinematic = false;

        if (m_RigidBody.SweepTest(Vector3.down, out RaycastHit hit_info, teleport_drop_height))
        {
            position.y = hit_info.point.y + surface_offset;
        }
        transform.position = position;
    }

    void ApplyRegularAggro(float delta_time)
    {
        float passive_decay = -m_CreatureAggroSettings.PassiveDecay;

        float light_aggro = 0f;
        if (m_PlayerSubmarine.LightsOn)
        {
            light_aggro = m_CreatureAggroSettings.LightAggro;
        }

        float speed_aggro = 0f;
        float min_speed_for_aggro = m_CreatureAggroSettings.MinMovementForSpeedAgro;
        if (m_PlayerSubmarine.currentSpeed.sqrMagnitude >= (min_speed_for_aggro * min_speed_for_aggro))
        {
            switch (m_PlayerSubmarine.currentGear)
            {
                case SubmarineController.MovementGear.SLOW:
                    speed_aggro = m_CreatureAggroSettings.LowSpeedAggro;
                    break;
                case SubmarineController.MovementGear.NORMAL:
                    speed_aggro = m_CreatureAggroSettings.MidSpeedAggro;
                    break;
                case SubmarineController.MovementGear.FAST:
                    speed_aggro = m_CreatureAggroSettings.HighSpeedAggro;
                    break;
            }
        }

        float height_aggro = 0f;
        float player_height = m_PlayerSubmarine.transform.position.y;
        if (player_height >= m_CreatureAggroSettings.MinHeightForAggro)
        {
            height_aggro = Mathf.Min(m_CreatureAggroSettings.HeightAggroPerMeter * (player_height - m_CreatureAggroSettings.MinHeightForAggro), m_CreatureAggroSettings.MaxHeightAggro);
        }

        float tractor_aggro = m_PlayerSubmarine.TractorBeamOn ? m_CreatureAggroSettings.TractorBeamAggro : 0f;

        float scrape_aggro = m_PlayerSubmarine.Scraping ? m_CreatureAggroSettings.TerrainScrapeAggro : 0f;

#if UNITY_EDITOR
        //LogHunterMessage($"[Hunter] Threat Tick. Decay: {passive_decay : #.##} | Lights: {light_aggro : #.##} | Speed: {speed_aggro : #.##} | Height: {height_aggro: #.##} | Tractor Beam: {tractor_aggro : #.##} | Scrape Aggro: {scrape_aggro : #.##}");
#endif

        PlayerAggro += (passive_decay + light_aggro + height_aggro + speed_aggro + tractor_aggro + scrape_aggro) * delta_time;
    }

    public void OnPlayerPickup()
    {
        PlayerAggro += m_CreatureAggroSettings.PickupCollectedAggro;
    }

    public void OnTerrainBump(bool isBig)
    {
        PlayerAggro += isBig ? m_CreatureAggroSettings.BigTerrainBumpAggro : m_CreatureAggroSettings.TerrainBumpAggro;
    }

    public void OnCreatureBump()
    {
        PlayerAggro += m_CreatureAggroSettings.SeaCreatureBumpAggro;
    }

    #region Unity Methods
    //Unity Methods
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            m_AudioSource = GetComponent<AudioSource>();
            m_RigidBody = GetComponent<Rigidbody>();
            m_BiteTriggerSphere = GetComponentInChildren<SphereCollider>();
            m_MeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            m_Animator = GetComponentInChildren<Animator>();

            //Initialise the state settings collection
            int num_states = Enum.GetValues(typeof(HunterState)).Length;
            m_AllStateSettings.Resize(num_states);

            m_AllStateSettings[(int)HunterState.Backstage]         = m_BackstageSettings;
            m_AllStateSettings[(int)HunterState.FrontstageIdle]    = m_FrontstageIdleSettings;
            m_AllStateSettings[(int)HunterState.FrontstageDistant] = m_FrontstageDistantSettings;
            m_AllStateSettings[(int)HunterState.Suspicious]        = m_FrontstageSuspiciousSettings;
            m_AllStateSettings[(int)HunterState.FrontstageClose]   = m_FrontstageCloseSettings;
            m_AllStateSettings[(int)HunterState.Attacking]         = m_AttackingSettings;
            m_AllStateSettings[(int)HunterState.Retreat]           = m_RetreatSettings;

            GoBackstage();
        }
        else
        {
            Debug.LogError($"Error: More than one hunter behaviour exists. Objects: {Instance.name} | {name}");
            Destroy(this);
        }
    }

    private void Start()
    {
        //Find the player
        m_PlayerSubmarine = FindObjectOfType<SubmarineController>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void FixedUpdate()
    {
        UpdatePeriodicEffects();
        MoveTowardsTarget();
        ApplyRegularAggro(Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other_collider)
    {
        if (m_CurrentState == HunterState.Attacking && AttackEnabled)
        {
            if (other_collider.gameObject == m_PlayerSubmarine.gameObject)
            {
                PlayerBitten(m_PlayerSubmarine);
            }
        }
    }

    #endregion

    #region Debug
    public static bool EnableLogging = false;

#if UNITY_EDITOR
    public void DebugTeleport(Vector3 position)
    {
        LeaveLimbo(position);
    }
#endif

    private void LogHunterMessage(string content)
    {
#if UNITY_EDITOR
        if (EnableLogging)
        {
            Debug.Log(content, this);
        }
#endif
    }

    #endregion
}
