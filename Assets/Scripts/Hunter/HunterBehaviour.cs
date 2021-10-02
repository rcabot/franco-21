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
    [SerializeField] private List<HunterState> m_States = new List<HunterState>();
    private HunterState                        m_CurrentState;
    private float                              m_TimeUntilPeriodEffect = 0f;
    private AudioSource                        m_AudioSource;
    private Rigidbody                          m_RigidBody;

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

    //Methods
    void HandleAttentionChanged()
    {
        Debug.Log($"Player Attention: {m_PlayerAttention}");

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
        m_CurrentState = state;

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
                m_TimeUntilPeriodEffect = m_CurrentState.PeriodTimeRange.RandomValue;

                PlayPeriodicSound();
                ApplyScreenShake(m_CurrentState.PeriodicScreenShakeMagnitude, m_CurrentState.PeriodicScreenShakeDuration);
            }
        }
    }

    private void PlayStateEnterSound()
    {
        m_AudioSource.clip = m_CurrentState.StateStartSounds.RandomSound;
        if (m_AudioSource.clip)
        {
            m_AudioSource.volume = m_CurrentState.StartSoundVolume;
            m_AudioSource.Play();
        }
    }

    private void PlayPeriodicSound()
    {
        m_AudioSource.clip = m_CurrentState.PeriodicSounds.RandomSound;
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

    //Unity Methods
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            m_AudioSource = GetComponent<AudioSource>();
            m_RigidBody = GetComponent<Rigidbody>();

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

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F2))
        {
            PlayerAttention += 5;
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            PlayerAttention -= 5;
        }
#endif
    }

    private void FixedUpdate()
    {
        UpdatePeriodicEffects();
    }
}
