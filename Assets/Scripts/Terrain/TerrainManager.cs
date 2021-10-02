using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    const int MERGE_WIDTH = 1;

    public static TerrainManager Instance = null;

    [SerializeField] TerrainDefinition Definition;
    private GameObject TerrainRoot;
    private List<TerrainTile> Tiles;

    public TerrainTile GetTile(Vector2Int index)
    {
        int offset = (Definition.EdgeTileCount * index.x) + index.y;
        if (offset < Tiles.Count)
        {
            return Tiles[offset];
        }

        return null;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError($"Error: Multiple terrain managers exist. Objects: {Instance.name} | {name}");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private static void MergeHeights(float[,] baseHeights, float[,] neighborHeights, int width, int height)
    {
        for(int currentRow = 0; currentRow < height; ++currentRow)
        {
            for(int currentColumn = 0; currentColumn < width; ++currentColumn)
            {
                float prevValue = neighborHeights[currentRow, currentColumn];
                float baseValue = baseHeights[currentRow, currentColumn];
                float newValue = (prevValue + baseValue) * 0.5f;
                neighborHeights[currentRow, currentColumn] = newValue;
            }
        }
    }

    private void StitchTiles(TerrainTile tileBase, TerrainTile tileNeighbor)
    {
        // Stitch neighbor using the base
        TerrainData baseData = tileBase.TerrainComponent.terrainData;
        TerrainData neighborData = tileNeighbor.TerrainComponent.terrainData;

        float[,] baseHeights;
        //float[,] neighborHeights;
        //int width = 0;
        //int height = 0;

        if (tileNeighbor.TileIndex.x > tileBase.TileIndex.x)
        {
            // Stitching with base on the left
            //width = MERGE_WIDTH;
            //height = baseData.heightmapResolution;
            baseHeights = baseData.GetHeights(baseData.heightmapResolution - MERGE_WIDTH, 0, MERGE_WIDTH, baseData.heightmapResolution);
            //neighborHeights = neighborData.GetHeights(0, 0, MERGE_WIDTH, baseData.heightmapResolution);
        }
        else 
        {
            // Stitching with base on the bottom
            //width = baseData.heightmapResolution;
            //height = MERGE_WIDTH;
            baseHeights = baseData.GetHeights(0, baseData.heightmapResolution - MERGE_WIDTH, baseData.heightmapResolution, MERGE_WIDTH);
            //neighborHeights = neighborData.GetHeights(0, 0, baseData.heightmapResolution, MERGE_WIDTH);
        }

        //MergeHeights(baseHeights, neighborHeights, width, height);
        //neighborData.SetHeightsDelayLOD(0, 0, neighborHeights);
        neighborData.SetHeightsDelayLOD(0, 0, baseHeights);
    }

    private void ConnectTiles()
    {
        // Go over all tiles, always only check the "left" and "bottom" neighbor (if available) and stitch to their height values
        List<Terrain> neighborList = new List<Terrain>();
        foreach (TerrainTile currentTile in Tiles)
        {
            neighborList.Clear();
            Vector2Int neighborIndex;

            if (currentTile.TileIndex.x > 0)
            {
                // Get the "left" neighbor
                neighborIndex = currentTile.TileIndex;
                --neighborIndex.x;

                TerrainTile leftNeighbor = GetTile(neighborIndex);
                neighborList.Add(leftNeighbor.TerrainComponent);

                StitchTiles(leftNeighbor, currentTile);
            }
            else
            {
                neighborList.Add(null);
            }

            if (currentTile.TileIndex.y < (Definition.EdgeTileCount - 1))
            {
                // Get the "top" neighbor
                neighborIndex = currentTile.TileIndex;
                ++neighborIndex.y;
                neighborList.Add(GetTile(neighborIndex).TerrainComponent);
            }
            else
            {
                neighborList.Add(null);
            }

            if (currentTile.TileIndex.x < (Definition.EdgeTileCount - 1))
            {
                // Get the "right" neighbor
                neighborIndex = currentTile.TileIndex;
                ++neighborIndex.x;
                neighborList.Add(GetTile(neighborIndex).TerrainComponent);
            }
            else
            {
                neighborList.Add(null);
            }

            if (currentTile.TileIndex.y > 0)
            {
                // Get the "bottom" neighbor
                neighborIndex = currentTile.TileIndex;
                --neighborIndex.y;

                TerrainTile bottomNeighbor = GetTile(neighborIndex);
                neighborList.Add(bottomNeighbor.TerrainComponent);

                StitchTiles(bottomNeighbor, currentTile);
            }
            else
            {
                neighborList.Add(null);
            }

            if((neighborList[0] != null) || (neighborList[3] != null))
            {
                // Apply the changes
                currentTile.TerrainComponent.terrainData.SyncHeightmap();
            }

            // Set the neighbors
            currentTile.TerrainComponent.SetNeighbors(neighborList[0], neighborList[1], neighborList[2], neighborList[3]);
            currentTile.TerrainComponent.Flush();
        }
    }

    // Start is called before the first frame update
    void Start()
    {        
        Tiles = new List<TerrainTile>();

        TerrainRoot = new GameObject("Terrain Root");

        float tileSize = Definition.TerrainSize / Definition.EdgeTileCount;
        float rowOffset = 0;
        for (int tileRow = 0; tileRow < Definition.EdgeTileCount; ++tileRow)
        {
            float columnOffset = 0;
            for(int tileCol = 0; tileCol < Definition.EdgeTileCount; ++tileCol)
            {
                TerrainTile currentTile = Instantiate(Definition.TilePrefab);
                currentTile.transform.parent = TerrainRoot.transform;
                currentTile.transform.position = new Vector3(columnOffset, 0, rowOffset);
                currentTile.name = string.Format("Terrain Tile ({0},{1})", tileRow, tileCol);

                currentTile.Init(Definition, new Vector2Int(tileRow, tileCol));
                Tiles.Add(currentTile);

                columnOffset += tileSize;
            }

            rowOffset += tileSize;
        }

        ConnectTiles();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
