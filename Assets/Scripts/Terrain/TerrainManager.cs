using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance = null;

    [SerializeField] TerrainDefinition Definition = null;
    private GameObject TerrainRoot;
    private List<TerrainTile> Tiles;

    struct Mountain
    {
        public Vector2 Position;
        public float Radius;
    }

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

    private void StitchTiles(TerrainTile tileBase, TerrainTile tileNeighbor)
    {
        // Stitch neighbor using the base
        TerrainData baseData = tileBase.TerrainComponent.terrainData;
        TerrainData neighborData = tileNeighbor.TerrainComponent.terrainData;

        float[,] baseHeights;
        if (tileNeighbor.TileIndex.x > tileBase.TileIndex.x)
        {
            // Stitching with base on the left
            baseHeights = baseData.GetHeights(baseData.heightmapResolution - 1, 0, 1, baseData.heightmapResolution);
        }
        else 
        {
            // Stitching with base on the bottom
            baseHeights = baseData.GetHeights(0, baseData.heightmapResolution - 1, baseData.heightmapResolution, 1);
        }

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
                leftNeighbor.TerrainComponent.terrainData.SyncHeightmap();
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
                bottomNeighbor.TerrainComponent.terrainData.SyncHeightmap();
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

    void CalculateTileMountainInfluence(float[,] heightmap, Vector2Int tileIndex, Mountain mountainData, TerrainData terrainData)
    {
        // Loop over the texels in the tile and check how much the mountain influences them
        float terrainMinOffset = -(Definition.TerrainSize * 0.5f);
        int resolution = terrainData.heightmapResolution * Definition.EdgeTileCount;
        float texelStepSize = Definition.TerrainSize / resolution;

        Vector2Int baseIndexOffset = new Vector2Int(tileIndex.x * terrainData.heightmapResolution, tileIndex.y * terrainData.heightmapResolution);
        for (int currentRow = 0; currentRow < terrainData.heightmapResolution; ++currentRow)
        {
            int baseRow = baseIndexOffset.x + currentRow;
            float texelYCoord = terrainMinOffset + (texelStepSize * baseRow);
            float mountainNoiseYCoord = (texelStepSize * baseRow) * Definition.MountainNoiseScale;

            for (int currentCol = 0; currentCol < terrainData.heightmapResolution; ++currentCol)
            {
                int baseColumn = baseIndexOffset.y + currentCol;
                float texelXCoord = terrainMinOffset + (texelStepSize * baseColumn);

                Vector2 texelPos = new Vector2(texelXCoord, texelYCoord);
                float mountainDistance = Vector2.Distance(texelPos, mountainData.Position);

                if (mountainDistance <= mountainData.Radius)
                {
                    float mountainScale = mountainDistance / mountainData.Radius;
                    float mountainNoiseXCoord = (texelStepSize * baseColumn) * Definition.MountainNoiseScale;

                    float baseValue = heightmap[baseRow, baseColumn];
                    float mountainValue = Mathf.PerlinNoise(mountainNoiseXCoord, mountainNoiseYCoord);
                    heightmap[baseRow, baseColumn] = baseValue * mountainScale + (1.0f - mountainScale) * mountainValue;
                }
            }
        }
    }

    void GenerateMountains(float[,] heightmap, TerrainData terrainData)
    {
        float tileSize = Definition.TerrainSize / Definition.EdgeTileCount;
        float terrainMinOffset = -(Definition.TerrainSize * 0.5f);

        bool[,] influenceGrid = new bool[Definition.EdgeTileCount, Definition.EdgeTileCount];
        for (int currentRow = 0; currentRow < Definition.EdgeTileCount; ++currentRow)
        {
            for(int currentColumn = 0; currentColumn < Definition.EdgeTileCount; ++currentColumn)
            {
                influenceGrid[currentRow, currentColumn] = false;
            }
        }

        int mountainCount = 0;
        int mountainCountMax = Mathf.Min(Definition.MountainCount, Definition.EdgeTileCount * Definition.EdgeTileCount);
        while(mountainCount < mountainCountMax)
        {
            int mountainRow = Random.Range(0, Definition.EdgeTileCount);
            int mountainColumn = Random.Range(0, Definition.EdgeTileCount);

            if(!influenceGrid[mountainRow, mountainColumn])
            {
                // Add a mountain
                Mountain mountainData = new Mountain();
                mountainData.Position = new Vector2(terrainMinOffset + ((mountainColumn + 0.5f) * tileSize), terrainMinOffset + ((mountainRow + 0.5f) * tileSize));
                mountainData.Radius = (tileSize * Definition.MountainAreaFactor) * 0.5f;

                GameObject mountainObj = new GameObject(string.Format("Mountain_{0}", mountainCount));
                mountainObj.transform.parent = TerrainRoot.transform;
                mountainObj.transform.position = new Vector3(mountainData.Position.x, 0, mountainData.Position.y);

                // Calculate the boundaries that need to be updated
                int startRow = Mathf.Max(mountainRow - 1, 0);
                int endRow = Mathf.Min(mountainRow + 2, Definition.EdgeTileCount);
                int startCol = Mathf.Max(mountainColumn - 1, 0);
                int endCol = Mathf.Min(mountainColumn + 2, Definition.EdgeTileCount);

                for (int currentRow = startRow; currentRow < endRow; ++currentRow)
                {
                    for (int currentColumn = startCol; currentColumn < endCol; ++currentColumn)
                    {
                        // Update the heightmap values within the tile
                        CalculateTileMountainInfluence(heightmap, new Vector2Int(currentRow, currentColumn), mountainData, terrainData);
                    }
                }

                // Update the influences
                influenceGrid[mountainRow, mountainColumn] = true;

                ++mountainCount;
            }
        }
    }

    float[,] GenerateHeightmap()
    {
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = Definition.Resolution.y;

        int resolution = terrainData.heightmapResolution * Definition.EdgeTileCount;
        float[,] heightmap = new float[resolution, resolution];

        // First perform the base pass
        for (int currentRow = 0; currentRow < resolution; ++currentRow)
        {
            float baseNoiseYCoord = currentRow * Definition.BaseNoiseScale;
            for (int currentCol = 0; currentCol < resolution; ++currentCol)
            {
                float baseNoiseXCoord = currentCol * Definition.BaseNoiseScale;
                float baseValue = Mathf.PerlinNoise(baseNoiseXCoord, baseNoiseYCoord) * Definition.BaseFactor;

                float heightValue = Mathf.Clamp(baseValue, 0.0f, 1.0f);
                heightmap[currentRow, currentCol] = heightValue;
            }
        }

        // Add mountains
        GenerateMountains(heightmap, terrainData);

        return heightmap;
    }

    // Start is called before the first frame update
    void Start()
    {        
        Tiles = new List<TerrainTile>();

        TerrainRoot = new GameObject("Terrain Root");

        // NOTE: need to use the resolution from the heightmap, since it gets adjusted (e.g 512 becomes 513, don't ask why...)
        float[,] baseHeightmap = GenerateHeightmap();

        float tileSize = Definition.TerrainSize / Definition.EdgeTileCount;
        float terrainMinOffset = -(Definition.TerrainSize * 0.5f);
        float rowOffset = terrainMinOffset;
        for (int tileRow = 0; tileRow < Definition.EdgeTileCount; ++tileRow)
        {
            float columnOffset = terrainMinOffset;
            for(int tileCol = 0; tileCol < Definition.EdgeTileCount; ++tileCol)
            {
                TerrainTile currentTile = Instantiate(Definition.TilePrefab);
                currentTile.transform.parent = TerrainRoot.transform;
                currentTile.transform.position = Vector3.right * rowOffset + Vector3.forward * columnOffset;
                currentTile.name = string.Format("Terrain Tile ({0},{1})", tileRow, tileCol);

                currentTile.Init(Definition, baseHeightmap, new Vector2Int(tileRow, tileCol));
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
