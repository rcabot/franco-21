using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class HunterAggroSettings
{
    [SerializeField] private float m_PassiveDecay = 1f;
    [SerializeField] private float m_LightAggro = 1f;
    [SerializeField] private float m_LowSpeedAggro = 1f;
    [SerializeField] private float m_MidSpeedAggro = 1f;
    [SerializeField] private float m_HighSpeedAggro = 1f;
    [SerializeField] private float m_MinMovementForSpeedAgro = 0.5f;
    [SerializeField] private float m_TractorBeamAggro = 1f;
    [SerializeField] private float m_PickupCollectedAggro = 1f;
    [SerializeField] private float m_LightOnCreatureAggro = 1f;
    [SerializeField] private float m_TerrainBumpAggro = 1f;
    [SerializeField] private float m_SeaCreatureBumpAggro = 1f;
    [SerializeField] private float m_HeightAggroStart = 100f;
    [SerializeField] private float m_HeightAggroPerMeter = 1f;

    public float PassiveDecay => m_PassiveDecay;
    public float LightAggro => m_LightAggro;
    public float LowSpeedAggro => m_LowSpeedAggro;
    public float MidSpeedAggro => m_MidSpeedAggro;
    public float HighSpeedAggro => m_HighSpeedAggro;
    public float MinMovementForSpeedAgro => m_MinMovementForSpeedAgro;
    public float TractorBeamAggro => m_TractorBeamAggro;
    public float PickupCollectedAggro => m_PickupCollectedAggro;
    public float LightOnCreatureAggro => m_LightOnCreatureAggro;
    public float TerrainBumpAggro => m_TerrainBumpAggro;
    public float SeaCreatureBumpAggro => m_SeaCreatureBumpAggro;
    public float MinHeightForAggro => m_HeightAggroStart;
    public float HeightAggroPerMeter => m_HeightAggroPerMeter;
}
