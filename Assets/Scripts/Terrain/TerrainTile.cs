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

    public void Init(TerrainDefinition definition, Vector2Int tileIndex)
    {
        _TileIndex = tileIndex;
        float tileWidth = definition.TerrainSize / definition.EdgeTileCount;
        transform.position = (Vector3.right * tileWidth * tileIndex.x) + (Vector3.forward * tileWidth * tileIndex.y);

        TerrainData terrainData = new TerrainData();
        terrainData.baseMapResolution = definition.Resolution.x;
        terrainData.heightmapResolution = definition.Resolution.y;
        terrainData.SetDetailResolution(32, 8);
        terrainData.size = new Vector3(tileWidth, definition.MaxHeight, tileWidth);

        // NOTE: need to use the resolution from the heightmap, since it gets adjusted (e.g 512 becomes 513, don't ask why...)
        float[,] heightmap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
        float scale = (definition.Resolution.y * 0.123f);
        Vector2 noiseOffset = new Vector2(tileIndex.x * scale, tileIndex.y * scale);

        for (int currentRow = 0; currentRow < terrainData.heightmapResolution; ++currentRow)
        {
            float noiseYCoord = noiseOffset.y + (currentRow / ((float)definition.Resolution.y)) * scale;
            for (int currentCol = 0; currentCol < terrainData.heightmapResolution; ++currentCol)
            {
                float noiseXCoord = noiseOffset.x + (currentCol / ((float)definition.Resolution.y) * scale);
                float heightValue = Mathf.Clamp(Mathf.PerlinNoise(noiseXCoord, noiseYCoord), 0.0f, 1.0f);
                heightmap[currentRow, currentCol] = heightValue;
            }
        }

        terrainData.SetHeights(0, 0, heightmap);
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
