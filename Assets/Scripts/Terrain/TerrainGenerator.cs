using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private Vector2Int HorizontalDimensions;
    [SerializeField] private Vector2 VerticalLimits;
    private GameObject TerrainObject;

    // Start is called before the first frame update
    void Start()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.size = new Vector3(HorizontalDimensions.x, VerticalLimits.y - VerticalLimits.x, HorizontalDimensions.y);
        terrainData.heightmapResolution = 512;
        terrainData.baseMapResolution = 1024;
        terrainData.SetDetailResolution(32, 8);

        float[,] heightMap = new float[HorizontalDimensions.x, HorizontalDimensions.y];
        for(int currentX = 0; currentX < HorizontalDimensions.x; ++currentX)
        {
            for(int currentY = 0; currentY < HorizontalDimensions.y; ++currentY)
            {
                heightMap[currentX, currentY] = Random.Range(VerticalLimits.x, VerticalLimits.y);
            }
        }

        terrainData.SetHeights(0, 0, heightMap);
        TerrainObject = Terrain.CreateTerrainGameObject(terrainData);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
