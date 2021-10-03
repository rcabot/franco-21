using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Franco Jam/Terrain Definition")]
public class TerrainDefinition : ScriptableObject
{
    [SerializeField, Tooltip("Size of the edge of the map")] private float _TerrainSize = 0f;
    [SerializeField, Tooltip("Number of tiles on an edge (i.e setting to N means the playable area is N x N)")] private int _EdgeTileCount = 0;
    [SerializeField, Tooltip("Heightmap resolution (default recommended)")] private Vector2Int _Resolution = Vector2Int.zero;
    [SerializeField, Tooltip("The maximum height of the generated terrain")] private float _MaxHeight = 1f;

    [SerializeField, Tooltip("Perlin noise scaling for the base terrain (higher --> more bumps)")] private float _BaseNoiseScale = 0.0123f;
    [SerializeField, Tooltip("Base terrain height factor (w.r.t max height)")] private float _BaseFactor = 0.3f;

    [SerializeField, Tooltip("Perlin noise scaling for the mountains (higher --> more bumps)")] private float _MountainNoiseScale = 0.123f;
    [SerializeField, Tooltip("Area of a mountain (factored w.r.t the size of a tile, e.g 0.5 --> it will be half as big as a tile")] private float _MountainAreaFactor = 0.5f;
    [SerializeField, Tooltip("Spawn this many mountains")] private int _MountainCount = 0;

    [SerializeField, Tooltip("Basin wall gradient steepness")] private float _BasinSteepness = 1.0f;
    [SerializeField, Tooltip("Basin wall gradient damping")] private float _BasinDamping = 1.0f;
    [SerializeField, Tooltip("Basin wall gradient offset")] private float _BasinOffset = 0.0f;

    [SerializeField, Tooltip("Number of flora patches to spawn per tile")] private int _FloraPatchPerTile = 0;
    [SerializeField, Tooltip("Number of flora objects in a patch")] private int _FloraPatchDensity = 0;

    [SerializeField] private TerrainTile _TilePrefab = null;
    [SerializeField] private Material _TileMaterial = null;
    [SerializeField] private GameObject[] _TreePrototypes;

    public float TerrainSize => _TerrainSize;
    public int EdgeTileCount => _EdgeTileCount;
    public Vector2Int Resolution => _Resolution;
    public float MaxHeight => _MaxHeight;

    public float BaseNoiseScale => _BaseNoiseScale;
    public float BaseFactor => _BaseFactor;

    public float MountainNoiseScale => _MountainNoiseScale;
    public float MountainAreaFactor => _MountainAreaFactor;
    public int MountainCount => _MountainCount;

    public float BasinSteepness => _BasinSteepness;
    public float BasinDamping => _BasinDamping;
    public float BasinOffset => _BasinOffset;

    public int FloraPatchPerTile => _FloraPatchPerTile;
    public int FloraPatchDensity => _FloraPatchDensity;

    public TerrainTile TilePrefab => _TilePrefab;
    public Material TileMaterial => _TileMaterial;
    public GameObject[] TreePrototypes => _TreePrototypes;
}
