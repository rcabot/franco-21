using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] TerrainDefinition Definition;
    private List<TerrainTile> Tiles;

    // Start is called before the first frame update
    void Start()
    {        
        Tiles = new List<TerrainTile>();

        float tileSize = Definition.TerrainSize / Definition.EdgeTileCount;
        float rowOffset = 0;
        for (int tileRow = 0; tileRow < Definition.EdgeTileCount; ++tileRow)
        {
            float columnOffset = 0;
            for(int tileCol = 0; tileCol < Definition.EdgeTileCount; ++tileCol)
            {
                TerrainTile currentTile = Instantiate(Definition.TilePrefab);
                currentTile.transform.parent = gameObject.transform;
                currentTile.transform.position = new Vector3(columnOffset, 0, rowOffset);
                currentTile.name = string.Format("Tile ({0},{1})", tileRow, tileCol);

                currentTile.Init(Definition, new Vector2Int(tileRow, tileCol));
                Tiles.Add(currentTile);

                columnOffset += tileSize;
            }

            rowOffset += tileSize;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
