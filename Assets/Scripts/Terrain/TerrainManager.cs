using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class TerrainManager : MonoBehaviour
{
    public static TerrainManager Instance = null;

    [SerializeField] private TerrainDefinition _Definition = null;
    private GameObject TerrainRoot;
    private List<TerrainTile> Tiles;

    public TerrainDefinition Definition => _Definition;

    public Rect PlayableTerrainArea => new Rect(-Vector2.one * Definition.TerrainSize * 0.5f,  Vector2.one * Definition.TerrainSize);

    public Rect TerrainAreaWithEdge { get { Vector2 half_terrain_size = Vector2.one * GetTerrainSize() * 0.5f; return new Rect(-half_terrain_size, half_terrain_size * 2f); } }

    public bool TerrainGenerated => Tiles != null && !Tiles.Empty();

    struct Mountain
    {
        public Vector2 Position;
        public float Radius;
    }

    public int GetEdgeTileCount()
    {
        return (Definition.EdgeTileCount + 2);
    }

    public TerrainTile GetTile(Vector2Int index)
    {
        int offset = (GetEdgeTileCount() * index.x) + index.y;
        if (offset < Tiles.Count)
        {
            return Tiles[offset];
        }

        return null;
    }

    private float GetTileSize()
    {
        return (Definition.TerrainSize / Definition.EdgeTileCount);
    }

    private float GetTerrainSize()
    {
        return GetTileSize() * GetEdgeTileCount();
    }

    public float GetTerrainElevation(Vector3 position)
    {
        // First check if the position is within the terrain boundaries (on the XZ plane)
        float halfTerrainSize = GetTerrainSize() * 0.5f;
        if ((Mathf.Abs(position.x) <= halfTerrainSize) && (Mathf.Abs(position.y) <= (halfTerrainSize)))
        {
            // Find which tile this point is on (use spatial hashing)
            float tileSize = GetTileSize();
            float terrainMinOffset = -(halfTerrainSize);

            Vector2 offset = new Vector2(position.x - terrainMinOffset, position.z - terrainMinOffset);
            Vector2Int tileIndex = new Vector2Int(Mathf.FloorToInt(offset.x / tileSize), Mathf.FloorToInt(offset.y / tileSize));

            // Get the tile-local coordinates, use the interpolation function
            TerrainTile hashedTile = GetTile(tileIndex);
            Vector2 localNormCoords = new Vector2((offset.x / tileSize) - tileIndex.x, (offset.y / tileSize) - tileIndex.y);
            return hashedTile.GetTerrainElevation(localNormCoords) + hashedTile.TerrainComponent.transform.position.y;
        }
        else
        {
            return 0.0f;
        }
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

        int heightmapRes = baseData.heightmapResolution;

        float[,] baseHeights;
        if (tileNeighbor.TileIndex.x > tileBase.TileIndex.x)
        {
            // Stitching with base on the left
            baseHeights = baseData.GetHeights(heightmapRes - 1, 0, 1, heightmapRes);
        }
        else 
        {
            // Stitching with base on the bottom
            baseHeights = baseData.GetHeights(0, heightmapRes - 1, heightmapRes, 1);
        }

        neighborData.SetHeightsDelayLOD(0, 0, baseHeights);
    }

    private void ConnectTiles()
    {
        Profiler.BeginSample("Connect Tiles");

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

        Profiler.EndSample();
    }

    private void GenerateFlora()
    {
        Profiler.BeginSample("Generate Flora");

        if ((Definition.TreePrototypes.Length <= 0)
            || (Definition.FloraPatchPerTile <= 0)
            || (Definition.FloraPatchDensity <= 0))
        {
            Debug.LogWarning("Flora generation error: incorrect parameter(s)!");
            return;
        }

        // Generate flora only on the playable area tiles
        for(int currentRow = 0; currentRow < Definition.EdgeTileCount; ++currentRow)
        {
            for(int currentCol = 0; currentCol < Definition.EdgeTileCount; ++currentCol)
            {
                TerrainTile currentTile = Tiles[((currentRow + 1) * Definition.EdgeTileCount) + (currentCol + 1)];
                currentTile.GenerateFlora(Definition);
            }
        }

        Profiler.EndSample();
    }

    float GetBasinGradient(float value)
    {
        float result = (Mathf.Exp(value * Definition.BasinSteepness) / Definition.BasinDamping) - Definition.BasinOffset;
        return Mathf.Clamp(result / (1.0f + result), 0.0f, 1.0f);
    }

    void CalculateBasinTile(float[,] heightmap, Vector2Int tileIndex, int heightmapRes)
    {
        Profiler.BeginSample("Calculate Basin Tile");

        // Loop over the texels in the tile and modify them to create a basin
        float terrainSize = GetTerrainSize();
        int edgeTileCount = GetEdgeTileCount();

        float terrainMinOffset = -(terrainSize * 0.5f);
        int resolution = heightmapRes * edgeTileCount;

        float texelStepSize = GetTerrainSize() / resolution;
        float tileSize = GetTileSize();

        Bounds bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(Definition.TerrainSize, 0.5f, Definition.TerrainSize));

        Vector2Int baseIndexOffset = new Vector2Int(tileIndex.x * heightmapRes, tileIndex.y * heightmapRes);
        for (int currentRow = 0; currentRow < heightmapRes; ++currentRow)
        {
            int baseRow = baseIndexOffset.x + currentRow;
            float texelYCoord = terrainMinOffset + (texelStepSize * baseRow);
            for (int currentCol = 0; currentCol < heightmapRes; ++currentCol)
            {
                int baseColumn = baseIndexOffset.y + currentCol;
                float texelXCoord = terrainMinOffset + (texelStepSize * baseColumn);

                float distanceToBounds = Mathf.Sqrt(bounds.SqrDistance(new Vector3(texelXCoord, 0, texelYCoord)));
                float factor = GetBasinGradient(distanceToBounds / tileSize);
                float baseValue = heightmap[baseRow, baseColumn];

                heightmap[baseRow, baseColumn] = factor + baseValue * (1.0f - factor);
            }
        }

        Profiler.EndSample();
    }

    void GenerateBasin(float[,] heightmap, TerrainData terrainData)
    {
        Profiler.BeginSample("Generate Basin");

        int edgeTileCount = GetEdgeTileCount();

        //Must be called on the main thread. Also accessing this is *really* slow.
        int heightmapRes = terrainData.heightmapResolution;

        Parallel.For(0, edgeTileCount, currentRow =>
        {
            for (int currentColumn = 0; currentColumn < edgeTileCount; ++currentColumn)
            {
                if ((currentRow == 0)
                    || (currentRow == (edgeTileCount - 1))
                    || (currentColumn == 0)
                    || (currentColumn == (edgeTileCount - 1)))
                {
                    CalculateBasinTile(heightmap, new Vector2Int(currentRow, currentColumn), heightmapRes);
                }
            }
        });

        Profiler.EndSample();
    }

    void CalculateTileMountainInfluence(float[,] heightmap, Vector2Int tileIndex, Mountain mountainData, TerrainData terrainData)
    {
        Profiler.BeginSample("Calculate Mountain Influence");

        // Loop over the texels in the tile and check how much the mountain influences them
        int heightmapResolution = terrainData.heightmapResolution;
        float terrainMinOffset = -(GetTerrainSize() * 0.5f);
        int resolution = terrainData.heightmapResolution * GetEdgeTileCount();
        float texelStepSize = GetTerrainSize() / resolution;

        Vector2Int baseIndexOffset = new Vector2Int(tileIndex.x * heightmapResolution, tileIndex.y * heightmapResolution);
        Parallel.For(0, heightmapResolution, currentRow =>
        {
            int baseRow = baseIndexOffset.x + currentRow;
            float texelYCoord = terrainMinOffset + (texelStepSize * baseRow);
            float mountainNoiseYCoord = (texelStepSize * baseRow) * Definition.MountainNoiseScale;

            for (int currentCol = 0; currentCol < heightmapResolution; ++currentCol)
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
        });

        Profiler.EndSample();
    }

    void GenerateMountains(float[,] heightmap, TerrainData terrainData)
    {
        Profiler.BeginSample("Generate Mountains");

        float tileSize = GetTileSize();
        float terrainMinOffset = -(GetTerrainSize() * 0.5f);

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
                mountainData.Position = new Vector2(terrainMinOffset + ((mountainColumn + 1.5f) * tileSize), terrainMinOffset + ((mountainRow + 1.5f) * tileSize));
                mountainData.Radius = (tileSize * Definition.MountainAreaFactor) * 0.5f;

                GameObject mountainObj = new GameObject($"Mountain_{mountainCount}");
                mountainObj.transform.parent = TerrainRoot.transform;
                mountainObj.transform.position = new Vector3(mountainData.Position.x, 0, mountainData.Position.y);

                // Calculate the boundaries that need to be updated
                int startRow = Mathf.Max(mountainRow - 1, -1);
                int endRow = Mathf.Min(mountainRow + 2, Definition.EdgeTileCount + 1);
                int startCol = Mathf.Max(mountainColumn - 1, -1);
                int endCol = Mathf.Min(mountainColumn + 2, Definition.EdgeTileCount + 1);

                for (int currentRow = startRow; currentRow < endRow; ++currentRow)
                {
                    for (int currentColumn = startCol; currentColumn < endCol; ++currentColumn)
                    {
                        // Update the heightmap values within the tile (offset the values, as we only apply this change within the "basin"
                        CalculateTileMountainInfluence(heightmap, new Vector2Int(currentRow + 1, currentColumn + 1), mountainData, terrainData);
                    }
                }

                // Update the influences
                influenceGrid[mountainRow, mountainColumn] = true;
                ++mountainCount;
            }
        }

        Profiler.EndSample();
    }

    float[,] GenerateHeightmap()
    {
        Profiler.BeginSample("Generate Heightmap");

        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = Definition.Resolution.y;

        int resolution = terrainData.heightmapResolution * GetEdgeTileCount();
        float[,] heightmap = new float[resolution, resolution];

        // First perform the base pass
        Parallel.For(0, resolution, currentRow =>
        {
            float baseNoiseYCoord = currentRow * Definition.BaseNoiseScale;

            //False sharing happens and hurts performance if this is also made parallel
            for (int currentCol = 0; currentCol < resolution; ++currentCol)
            {
                float baseNoiseXCoord = currentCol * Definition.BaseNoiseScale;
                float baseValue = Mathf.PerlinNoise(baseNoiseXCoord, baseNoiseYCoord) * Definition.BaseFactor;

                float heightValue = Mathf.Clamp01(baseValue);
                heightmap[currentRow, currentCol] = heightValue;
            }
        });

        // Add basin & mountains
        GenerateBasin(heightmap, terrainData);
        GenerateMountains(heightmap, terrainData);

        Profiler.EndSample();

        return heightmap;
    }

    // Start is called before the first frame update
    void Start()
    {
        Profiler.BeginSample("Init Terrain");

        Tiles = new List<TerrainTile>();

        TerrainRoot = new GameObject("Terrain Root");

        // NOTE: need to use the resolution from the heightmap, since it gets adjusted (e.g 512 becomes 513, don't ask why...)
        float[,] baseHeightmap = GenerateHeightmap();

        float tileSize = GetTileSize();
        float terrainMinOffset = -(GetTerrainSize() * 0.5f);
        float rowOffset = terrainMinOffset;
        int edgeTileCount = GetEdgeTileCount();

        for (int tileRow = 0; tileRow < edgeTileCount; ++tileRow)
        {
            float columnOffset = terrainMinOffset;
            for(int tileCol = 0; tileCol < edgeTileCount; ++tileCol)
            {
                TerrainTile currentTile = Instantiate(Definition.TilePrefab);
                currentTile.transform.parent = TerrainRoot.transform;
                currentTile.transform.position = Vector3.right * rowOffset + Vector3.forward * columnOffset;
                currentTile.name = string.Format($"Terrain Tile ({tileRow},{tileCol})");

                currentTile.Init(Definition, baseHeightmap, new Vector2Int(tileRow, tileCol));
                Tiles.Add(currentTile);

                columnOffset += tileSize;
            }

            rowOffset += tileSize;
        }

        ConnectTiles();
        GenerateFlora();
    }
}
