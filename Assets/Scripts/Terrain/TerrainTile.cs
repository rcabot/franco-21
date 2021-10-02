using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTile : MonoBehaviour
{
    private Vector2Int _TileIndex;
    private GameObject TerrainObject;

    public Vector2Int TileIndex
    {
        get { return _TileIndex; }
    }

    public void Init(TerrainDefinition definition, Vector2Int tileIndex)
    {
        _TileIndex = tileIndex;
        transform.position = new Vector3(tileIndex.x * definition.TerrainSize, 0, tileIndex.y * definition.TerrainSize);

        float[,] heightmap = new float[definition.Resolution.y, definition.Resolution.y];

        for (int currentRow = 0; currentRow < definition.Resolution.y; ++currentRow)
        {
            for (int currentCol = 0; currentCol < definition.Resolution.y; ++currentCol)
            {
                heightmap[currentRow, currentCol] = Random.Range(0.0f, 1.0f);
            }
        }

        TerrainData terrainData = new TerrainData();
        terrainData.baseMapResolution = definition.Resolution.x;
        terrainData.heightmapResolution = definition.Resolution.y;
        terrainData.SetDetailResolution(32, 8);
        terrainData.size = new Vector3(definition.TerrainSize, definition.MaxHeight, definition.TerrainSize);

        terrainData.SetHeights(0, 0, heightmap);
        TerrainObject = Terrain.CreateTerrainGameObject(terrainData);
        TerrainObject.transform.SetParent(transform, false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
