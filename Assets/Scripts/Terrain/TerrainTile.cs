using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainTile : MonoBehaviour
{
    private Vector2Int _TileIndex;
    private GameObject TerrainObject;
    private Terrain _TerrainComponent;

    public Terrain TerrainComponent => _TerrainComponent;

    public Vector2Int TileIndex => _TileIndex;

    public void Init(TerrainDefinition definition, float[,] baseHeightmap, Vector2Int tileIndex)
    {
        _TileIndex = tileIndex;
        float tileWidth = definition.TerrainSize / definition.EdgeTileCount;

        // Generate the local heightmap
        TerrainData terrainData = new TerrainData();
        terrainData.baseMapResolution = definition.Resolution.x;
        terrainData.heightmapResolution = definition.Resolution.y;
        terrainData.SetDetailResolution(32, 8);
        terrainData.size = new Vector3(tileWidth, definition.MaxHeight, tileWidth);

        float[,] tileHeightmap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];

        // FIXME: do this via Array.Copy!!!
        Vector2Int baseIndexOffset = new Vector2Int(tileIndex.x * terrainData.heightmapResolution, tileIndex.y * terrainData.heightmapResolution);
        for (int currentRow = 0; currentRow < terrainData.heightmapResolution; ++currentRow)
        {
            int baseRow = baseIndexOffset.y + currentRow;
            for (int currentCol = 0; currentCol < terrainData.heightmapResolution; ++currentCol)
            {
                int baseColumn = baseIndexOffset.x + currentCol;
                tileHeightmap[currentRow, currentCol] = baseHeightmap[baseRow, baseColumn];
            }
        }

        // Generate the terrain object
        terrainData.SetHeights(0, 0, tileHeightmap);
        TerrainObject = Terrain.CreateTerrainGameObject(terrainData);
        TerrainObject.transform.SetParent(transform, false);

        _TerrainComponent = TerrainObject.GetComponent<Terrain>();
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
