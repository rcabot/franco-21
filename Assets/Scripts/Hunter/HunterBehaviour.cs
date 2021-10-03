using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(Rigidbody))]
public class HunterBehaviour : MonoBehaviour
{
    //Members
    private int                                m_PlayerAggro = 0;
    private float                              m_TimeUntilPeriodEffect = 0f;
    private AudioSource                        m_AudioSource;
    private Rigidbody                          m_RigidBody;
    private SphereCollider                     m_BiteTriggerSphere;
    private SkinnedMeshRenderer                m_MeshRenderer;
    private HunterStateSettings                m_CurrentStateSettings;
    private HunterState                        m_CurrentState;

    private Vector3                            m_Velocity;
    [SerializeField] private Vector3           m_MoveTarget;

    [Header("Physics")]
    [SerializeField] private float             m_Acceleration;
    [SerializeField] private float             m_TurnSpeed;
    [SerializeField] private float             m_BiteKnockbackForce = 40f;

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

    //Populated in awake. /!\ REMEMBER TO UPDATE THIS IF YOU ADD A NEW STATE /!\ 
    List<HunterStateSettings>                    m_AllStateSettings = new List<HunterStateSettings>();

    //Events
    public event EventHandler<int>                                            OnAggroChanged;
    public event EventHandler<KeyValuePair<HunterState, HunterStateSettings>> OnStateChanged;

    //Properties
    public static HunterBehaviour Instance { get; private set; }
    public int PlayerAggro
    {
        get => m_PlayerAggro;
        set { if (m_PlayerAggro != value) { m_PlayerAggro = value; HandleAggroChanged(); } }
    }

    public int MaxPlayerAggro => m_MaxPlayerAggro;

    public HunterStateSettings CurrentStateSettings => m_CurrentStateSettings;
    public HunterState         CurrentState => m_CurrentState;

    //Methods
    public void ForceSetState(HunterState state)
    {
        LogHunterMessage($"[Hunter] Forced state change: {state}");
        EnterState(state);
    }

    private void HandleAggroChanged()
    {
        LogHunterMessage($"[Hunter] Player Aggro: {m_PlayerAggro}");

        OnAggroChanged?.Invoke(this, m_PlayerAggro);

        //If the creature is attacking or retreating then aggro changes are ignored
        switch (CurrentState)
        {
            case HunterState.Attacking:
            case HunterState.Retreat:
                return;
        }

        int TriggerAggroForStateIfValid(HunterStateSettings state_settings)
        {
            return state_settings.ActivationAggro <= m_PlayerAggro ? state_settings.ActivationAggro : int.MinValue;
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
        EnterState(HunterState.Retreat);
    }

    //Woo coroutines
    private void StartStateBehaviour()
    {
        switch (CurrentState)
        {
            case HunterState.Backstage:
                GoBackstage();
                break;
            case HunterState.FrontstageIdle:
                StartCoroutine(FrontstageIdle());
                break;
            case HunterState.FrontstageDistant:
                StartCoroutine(FrontstageDistant());
                break;
            case HunterState.Suspicious:
                StartCoroutine(FrontstageSuspicious());
                break;
            case HunterState.FrontstageClose:
                StartCoroutine(FrontstageClose());
                break;
            case HunterState.Attacking:
                StartCoroutine(Attacking());
                break;
            case HunterState.Retreat:
                StartCoroutine(Retreat());
                break;
            default:
                LogHunterMessage("[Hunter] Unknown follow state. Missing code");
                break;
        }
    }

    private IEnumerator FrontstageIdle()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (CurrentState != HunterState.FrontstageIdle)
            {
                break;
            }

            //Idle behaviour per-tick
        }
    }

    private IEnumerator FrontstageDistant()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (CurrentState != HunterState.FrontstageDistant)
            {
                break;
            }

            //Frontstage Distant behaviour per-tick
        }
    }

    private IEnumerator FrontstageSuspicious()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (CurrentState != HunterState.Suspicious)
            {
                break;
            }

            //Suspicious behaviour per-tick
        }
    }

    private IEnumerator FrontstageClose()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (CurrentState != HunterState.FrontstageClose)
            {
                break;
            }

            //Frontstage Close behaviour per-tick
        }
    }

    private IEnumerator Attacking()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (CurrentState != HunterState.Attacking)
            {
                break;
            }

            //Attacking behaviour per-tick
        }
    }

    private IEnumerator Retreat()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (CurrentState != HunterState.Retreat)
            {
                break;
            }

            //Retreating behaviour per-tick
        }
    }

    private void GoBackstage()
    {
        EnterLimbo();
        transform.position = m_BackstagePosition;
        m_PlayerAggro = 0;
    }

    private void EnterLimbo()
    {
        m_RigidBody.isKinematic = true;
        m_MeshRenderer.enabled = false;
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


            EnterState(HunterState.Backstage);
        }
        else
        {
            Debug.LogError($"Error: More than one hunter behaviour exists. Objects: {Instance.name} | {name}");
            Destroy(this);
        }
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
    }

    private void OnTriggerEnter(Collider other_collider)
    {
        if (m_CurrentState == HunterState.Attacking)
        {
            SubmarineController player_sub = other_collider.GetComponent<SubmarineController>();
            if (player_sub != null)
            {
                PlayerBitten(player_sub);
            }
        }
    }

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
