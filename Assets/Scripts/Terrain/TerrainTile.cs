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

    public float GetTerrainElevation(Vector2 position)
    {
        return _TerrainComponent.terrainData.GetInterpolatedHeight(position.x, position.y);
    }

    public void GenerateFlora(TerrainDefinition definition)
    {
        List<TreePrototype> prototypeList = new List<TreePrototype>();
        foreach(GameObject treeProtoPrefab in definition.TreePrototypes)
        {
            TreePrototype newTreeProto = new TreePrototype();
            newTreeProto.prefab = treeProtoPrefab;
            prototypeList.Add(newTreeProto);
        }

        _TerrainComponent.terrainData.treePrototypes = prototypeList.ToArray();
        _TerrainComponent.terrainData.RefreshPrototypes();

        float patchRadius = Mathf.Clamp(1.0f / definition.FloraPatchPerTile, 0.1f, 0.4f);
        for (int patchCount = 0; patchCount < definition.FloraPatchPerTile; ++patchCount)
        {
            Vector2 patchOrigin = new Vector2(Random.Range(patchRadius, 1.0f - patchRadius), Random.Range(patchRadius, 1.0f - patchRadius));

            for (int floraCount = 0; floraCount < definition.FloraPatchDensity; ++floraCount)
            {
                TreeInstance newTreeInstance = new TreeInstance();
                newTreeInstance.prototypeIndex = Random.Range(0, definition.TreePrototypes.Length);
                newTreeInstance.position = new Vector3(patchOrigin.x + Random.Range(-patchRadius, patchRadius), 0, patchOrigin.y + Random.Range(-patchRadius, patchRadius));
                newTreeInstance.heightScale = Random.Range(0.6f, 1.0f);
                newTreeInstance.widthScale = Random.Range(0.6f, 1.0f);
                _TerrainComponent.AddTreeInstance(newTreeInstance);
            }
        }

        TerrainComponent.Flush();
    }

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
        TerrainObject.layer = LayerMask.NameToLayer("Terrain");

        _TerrainComponent = TerrainObject.GetComponent<Terrain>();
        _TerrainComponent.materialTemplate = definition.TileMaterial;
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
