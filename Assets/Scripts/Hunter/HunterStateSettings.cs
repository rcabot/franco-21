using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HunterState
{
    Backstage,
    FrontstageIdle,
    FrontstageDistant,
    Suspicious,
    FrontstageClose,
    Attacking,
    Retreat
}

[Serializable]
public class HunterStateSettings
{
    [Range(0, 100), SerializeField, Tooltip("Minimum aggro the player must achieve to enter this state")]
    private int                                    m_ActivationAggro = 0;

    [Range(0, 100), SerializeField, Tooltip("Distance from the player that the state action can be performed")]
    private float                                  m_ActionDistance = 10f;

    [SerializeField, RangeBeginEndAttribute(0f, 1000f), Tooltip("Distance range the creature will try to maintain from the player")]
    private RangeFloat                             m_PlayerDistanceRange = new RangeFloat(50f, 100f);

    [SerializeField, RangeBeginEndAttribute(-100f, 100f), Tooltip("Height offset range that the creature may appear at when spawning near the player")]
    private RangeFloat                             m_PlayerHeightOffset = new RangeFloat(-1f, 1f);

    [SerializeField, Tooltip("Speed of acceleration the creature has in this state")]
    private float                                  m_Acceleration = 10f;

    [Header("Start Effects")]
    [SerializeField] private SoundBank             m_StartSounds = null;
    [Range(0f, 1f), SerializeField] private float  m_StartVolumeScale = 1f;
    [Range(0f, 2f), SerializeField] private float  m_StartScreenShakeMagnitude = 0f;
    [Range(0f, 10f), SerializeField] private float m_StartScreenShakeDuration = 0f;

    [Header("Periodic Effects")]
    [SerializeField] private SoundBank             m_PeriodicSounds = null;
    [Range(0f, 1f), SerializeField] private float  m_PeriodicSoundScale = 1f;
    [Range(0f, 1f), SerializeField] private float  m_PeriodicScreenShakeMagnitude = 0f;
    [Range(0f, 10f), SerializeField] private float m_PeriodicScreenShakeDuration = 0f;

    [SerializeField, RangeBeginEndAttribute(0f, 60f), Tooltip("Random time window (sec) between periodic effects")]
    private RangeFloat                             m_PeriodTimeRange = new RangeFloat(0f, 0f);

    public int               ActivationAggro              => m_ActivationAggro;
    public float             ActionDistance               => m_ActionDistance;
    public RangeFloat        PlayerDistanceRange          => m_PlayerDistanceRange;
    public RangeFloat        PlayerHeightOffset           => m_PlayerHeightOffset;
    public float             Acceleration                 => m_Acceleration;

    public SoundBank         StateStartSounds             => m_StartSounds;
    public float             StartSoundVolume             => m_StartVolumeScale;
    public float             StartScreenShakeMagnitude    => m_StartScreenShakeMagnitude;
    public float             StartScreenShakeDuration     => m_StartScreenShakeDuration;

    public SoundBank         PeriodicSounds               => m_PeriodicSounds;
    public float             PeriodicSoundVolumeScale     => m_PeriodicSoundScale;
    public float             PeriodicScreenShakeMagnitude => m_PeriodicScreenShakeMagnitude;
    public float             PeriodicScreenShakeDuration  => m_PeriodicScreenShakeDuration;
    public RangeFloat        PeriodTimeRange              => m_PeriodTimeRange;
}
