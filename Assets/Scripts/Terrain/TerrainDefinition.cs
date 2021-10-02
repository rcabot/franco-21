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

    [SerializeField] private float _BaseNoiseScale = 0.0123f;
    [SerializeField] private float _BaseFactor = 0.3f;

    [SerializeField] private float _MountainNoiseScale = 0.123f;
    [SerializeField] private float _MountainAreaFactor = 0.5f;
    [SerializeField] private int _MountainCount = 0;

    [SerializeField] private TerrainTile _TilePrefab = null;

    public float TerrainSize => _TerrainSize;
    public int EdgeTileCount => _EdgeTileCount;
    public Vector2Int Resolution => _Resolution;
    public float MaxHeight => _MaxHeight;

    public float BaseNoiseScale => _BaseNoiseScale;
    public float BaseFactor => _BaseFactor;

    public float MountainNoiseScale => _MountainNoiseScale;
    public float MountainAreaFactor => _MountainAreaFactor;
    public int MountainCount => _MountainCount;

    public TerrainTile TilePrefab => _TilePrefab;
}
