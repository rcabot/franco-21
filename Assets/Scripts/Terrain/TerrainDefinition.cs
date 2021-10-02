using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Franco Jam/Terrain Definition")]
public class TerrainDefinition : ScriptableObject
{
    [SerializeField] private float _TerrainSize;
    [SerializeField] private int _EdgeTileCount;
    [SerializeField] private Vector2Int _Resolution;
    [SerializeField] private float _MaxHeight;

    [SerializeField] private TerrainTile _TilePrefab;

    public float TerrainSize => _TerrainSize;
    public int EdgeTileCount => _EdgeTileCount;
    public Vector2Int Resolution => _Resolution;
    public float MaxHeight => _MaxHeight;
    public TerrainTile TilePrefab => _TilePrefab;
}
