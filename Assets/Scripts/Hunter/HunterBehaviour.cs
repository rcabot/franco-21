using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource), typeof(Rigidbody))]
public class HunterBehaviour : MonoBehaviour
{
    //Members
    private int                                m_PlayerAttention = 0;
    private HunterState                        m_CurrentState;
    private float                              m_TimeUntilPeriodEffect = 0f;
    private AudioSource                        m_AudioSource;
    private Rigidbody                          m_RigidBody;
    private SphereCollider                     m_BiteTriggerSphere;
    private SkinnedMeshRenderer                m_MeshRenderer;
    private HunterFollowState                  m_FollowState;

    private Vector3                            m_Velocity;
    [SerializeField] private Vector3           m_MoveTarget;

    [Header("Physics")]
    [SerializeField] private float             m_Acceleration;
    [SerializeField] private float             m_TurnSpeed;
    [SerializeField] private float             m_BiteKnockbackForce = 40f;

    [Header("Behaviour")]
    [SerializeField, Range(1f, 100f), Tooltip("Distance that it's safe for the creature to appear/disappear at even if infront of the player camera")]
    private float                              m_SafeVisibilityDistance = 10f;
    [SerializeField] private Vector3           m_BackstagePosition = new Vector3(0f, 0f, -10000f);

    [SerializeField] private List<HunterState> m_States = new List<HunterState>();

    //Events
    public event EventHandler<int>             OnAttentionChanged;
    public event EventHandler<HunterState>     OnStateChanged;

    //Properties
    public static HunterBehaviour Instance { get; private set; }
    public int PlayerAttention
    {
        get => m_PlayerAttention;
        set { if (m_PlayerAttention != value) { m_PlayerAttention = value; HandleAttentionChanged(); } }
    }

    public HunterState        CurrentState => m_CurrentState;
    public HunterFollowState  FollowState => m_FollowState;

    //Methods
    public void ForceSetState(HunterState state)
    {
        LogHunterMessage($"[Hunter] Forced state change: {state.name}");

        if (!m_States.Contains(state))
        {
            m_States.Add(state);
        }

        EnterState(state);
    }

    private void HandleAttentionChanged()
    {
        LogHunterMessage($"[Hunter] Player Attention: {m_PlayerAttention}");

        //If the creature is attacking or retreating then attention changes are ignored
        switch (FollowState)
        {
            case HunterFollowState.Attacking:
            case HunterFollowState.Retreat:
                return;
        }

        int GetStateAttentionIfValid(HunterState state)
        {
            return state.MinimumAttention <= m_PlayerAttention ? state.MinimumAttention : int.MinValue;
        }

        OnAttentionChanged?.Invoke(this, m_PlayerAttention);

        HunterState new_state = m_States.Aggregate((a, b) => GetStateAttentionIfValid(a) < GetStateAttentionIfValid(b) ? b : a);
        if (new_state != null && new_state != m_CurrentState)
        {
            EnterState(new_state);
        }
    }

    private void EnterState(HunterState state)
    {
        LogHunterMessage($"[Hunter] Entering State  {state.name}");

        m_CurrentState = state;
        SetFollowBehaviour(m_CurrentState.FollowState);

        //Apply enter effects
        PlayStateEnterSound();
        ApplyScreenShake(state.StartScreenShakeMagnitude, state.StartScreenShakeDuration);

        //Setup the period effects
        m_TimeUntilPeriodEffect = state.PeriodTimeRange.RandomValue;

        OnStateChanged?.Invoke(this, state);
    }

    private void UpdatePeriodicEffects()
    {
        if (m_TimeUntilPeriodEffect > 0f)
        {
            m_TimeUntilPeriodEffect -= Time.deltaTime;

            if (m_TimeUntilPeriodEffect <= 0f)
            {
                if (m_CurrentState != null)
                {
                    m_TimeUntilPeriodEffect = m_CurrentState.PeriodTimeRange.RandomValue;

                    PlayPeriodicSound();
                    ApplyScreenShake(m_CurrentState.PeriodicScreenShakeMagnitude, m_CurrentState.PeriodicScreenShakeDuration);

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
        m_AudioSource.clip = m_CurrentState.StateStartSounds?.RandomSound;
        if (m_AudioSource.clip)
        {
            m_AudioSource.volume = m_CurrentState.StartSoundVolume;
            m_AudioSource.Play();
        }
    }

    private void PlayPeriodicSound()
    {
        m_AudioSource.clip = m_CurrentState.PeriodicSounds?.RandomSound;
        if (m_AudioSource.clip)
        {
            m_AudioSource.volume = m_CurrentState.PeriodicSoundVolumeScale;
            m_AudioSource.Play();
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
        HunterState retreat_state = m_States.FirstOrDefault(s => s.FollowState == HunterFollowState.Retreat);
        if (retreat_state != null)
        {
            EnterState(retreat_state);
        }
        else
        {
            //Even if there's no state defined for retreat. Force it
            SetFollowBehaviour(HunterFollowState.Retreat);
        }
    }

    //Woo coroutines
    private void SetFollowBehaviour(HunterFollowState new_follow_state)
    {
        m_FollowState = new_follow_state;

        switch (FollowState)
        {
            case HunterFollowState.Backstage:
                GoBackstage();
                break;
            case HunterFollowState.FrontstageIdle:
                StartCoroutine(FrontstageIdle());
                break;
            case HunterFollowState.FrontstageDistant:
                StartCoroutine(FrontstageDistant());
                break;
            case HunterFollowState.Suspicious:
                StartCoroutine(FrontstageSuspicious());
                break;
            case HunterFollowState.FrontstageClose:
                StartCoroutine(FrontstageClose());
                break;
            case HunterFollowState.Attacking:
                StartCoroutine(Attacking());
                break;
            case HunterFollowState.Retreat:
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

            if (FollowState != HunterFollowState.FrontstageIdle)
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

            if (FollowState != HunterFollowState.FrontstageDistant)
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

            if (FollowState != HunterFollowState.Suspicious)
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

            if (FollowState != HunterFollowState.FrontstageClose)
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

            if (FollowState != HunterFollowState.Attacking)
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

            if (FollowState != HunterFollowState.Retreat)
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
        m_PlayerAttention = 0;
    }

    private void EnterLimbo()
    {
        m_RigidBody.isKinematic = true;
        m_MeshRenderer.enabled = false;
    }

    private void LeaveLimbo(Vector3 position)
    {
        //Teleport the creature way above the target position and then move them down with the physics engine.
        transform.position = position + Vector3.up * 1000f;
        m_MeshRenderer.enabled = true;
        m_RigidBody.isKinematic = false;

        m_RigidBody.MovePosition(position);
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

            HunterState default_state = m_States.FirstOrDefault(s => s.IsDefault);
            if (default_state)
            {
                EnterState(default_state);
            }
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
        SubmarineController player_sub = other_collider.GetComponent<SubmarineController>();
        if (player_sub != null)
        {
            PlayerBitten(player_sub);
        }
    }

    #region Debug
    public static bool EnableLogging = false;

#if UNITY_EDITOR
    public void DebugTeleport(Vector3 position)
    {
        LeaveLimbo(position);
    }

    public void DebugForceBackstage()
    {
        HunterState backstage_state = m_States.FirstOrDefault(s => s.FollowState == HunterFollowState.Backstage);
        if (backstage_state)
        {
            ForceSetState(backstage_state);
        }
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
