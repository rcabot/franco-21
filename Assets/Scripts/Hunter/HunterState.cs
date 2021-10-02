using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HunterFollowState
{
    Backstage,
    FrontstageIdle,
    FrontstageDistant,
    Suspicious,
    FrontstageClose,
    Attacking
}

[CreateAssetMenu(menuName = "Franco Jam/Hunter State")]
public class HunterState : ScriptableObject
{
    [Header("General")]
    [SerializeField] private bool                  m_default_state;

    [SerializeField, Tooltip("Minimum attention required for the hunter to enter this state")]
    private int                                    m_MinimumAttention;

    [SerializeField]
    private HunterFollowState                      m_FollowState = HunterFollowState.Backstage;


    [Header("Start Effects")]
    [SerializeField] private SoundBank             m_StartSounds;
    [Range(0f, 1f), SerializeField] private float  m_StartVolumeScale = 1f;
    [Range(0f, 2f), SerializeField] private float  m_StartScreenShakeMagnitude = 0f;
    [Range(0f, 10f), SerializeField] private float m_StartScreenShakeDuration = 0f;

    [Header("Periodic Effects")]
    [SerializeField] private SoundBank             m_PeriodicSounds;
    [Range(0f, 1f), SerializeField] private float  m_PeriodicSoundScale = 1f;
    [Range(0f, 1f), SerializeField] private float  m_PeriodicScreenShakeMagnitude = 0f;
    [Range(0f, 10f), SerializeField] private float m_PeriodicScreenShakeDuration = 0f;

    [SerializeField, RangeBeginEndAttribute(0f, 60f), Tooltip("Random time window (sec) between periodic effects")]
    private RangeFloat                             m_PeriodTimeRange = new RangeFloat(0f, 1f);

    public bool              IsDefault                    => m_default_state;
    public int               MinimumAttention             => m_MinimumAttention;
    public HunterFollowState FollowState                  => m_FollowState;

    public SoundBank         StateStartSounds             => m_StartSounds;
    public float             StartSoundVolume        => m_StartVolumeScale;
    public float             StartScreenShakeMagnitude    => m_StartScreenShakeMagnitude;
    public float             StartScreenShakeDuration     => m_StartScreenShakeDuration;

    public SoundBank         PeriodicSounds               => m_PeriodicSounds;
    public float             PeriodicSoundVolumeScale     => m_PeriodicSoundScale;
    public float             PeriodicScreenShakeMagnitude => m_PeriodicScreenShakeMagnitude;
    public float             PeriodicScreenShakeDuration  => m_PeriodicScreenShakeDuration;
    public RangeFloat        PeriodTimeRange              => m_PeriodTimeRange;
}
