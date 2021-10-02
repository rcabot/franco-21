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
        transform.position = (Vector3.right * definition.TerrainSize * tileIndex.x) + (Vector3.forward * definition.TerrainSize * tileIndex.y);

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
