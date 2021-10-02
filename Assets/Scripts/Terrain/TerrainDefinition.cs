using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Franco Jam/Terrain Definition")]
public class TerrainDefinition : ScriptableObject
{
    [SerializeField] private float _TerrainSize = 0f;
    [SerializeField] private int _EdgeTileCount = 0;
    [SerializeField] private Vector2Int _Resolution = Vector2Int.zero;
    [SerializeField] private float _MaxHeight = 1f;

    [SerializeField] private float _BaseNoiseScale = 1f;
    [SerializeField] private float _DetailNoiseScale = 1f;
    [SerializeField] private float _DetailFactor = 1f;
    [SerializeField] private float _DetailDamping = 1f;

    [SerializeField] private TerrainTile _TilePrefab = null;

    public float TerrainSize => _TerrainSize;
    public int EdgeTileCount => _EdgeTileCount;
    public Vector2Int Resolution => _Resolution;
    public float MaxHeight => _MaxHeight;
    public float BaseNoiseScale => _BaseNoiseScale;
    public float DetailNoiseScale => _DetailNoiseScale;
    public float DetailFactor => _DetailFactor;
    public float DetailDamping => _DetailDamping;
    public TerrainTile TilePrefab => _TilePrefab;
}
