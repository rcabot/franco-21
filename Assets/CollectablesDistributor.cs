using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class CollectablesDistributor : MonoBehaviour
{
    public BoxCollider DistributionArea;
    public int TilesToLitter;
    public int AmountToDistribute;
    public WeightedPrefab[] Prefabs;
    public List<Collectable> SpawnedCollectables;

    IEnumerator CoInitialise()
    {
        //Wait for the terrain to be setup
        while (!(TerrainManager.Instance?.TerrainGenerated ?? false))
        {
            yield return new WaitForFixedUpdate();
        }

        // Randomly select a given number of tiles and put junk on them
        int litteredTiles = 0;
        int tileCount = TerrainManager.Instance.GetEdgeTileCount();
        int playAreaTileCount = tileCount - 2;
        int tilesToLitter = Mathf.Min(TilesToLitter, playAreaTileCount * playAreaTileCount);

        // Prepare a LUT so we only litter a tile once
        bool[,] doneTileLUT = new bool[tileCount - 1, tileCount - 1];
        for (int currentRow = 0; currentRow < (tileCount - 1); ++currentRow)
        {
            for (int currentCol = 0; currentCol < (tileCount - 1); ++currentCol)
            {
                doneTileLUT[currentRow, currentCol] = false;
            }
        }

        while (litteredTiles < tilesToLitter)
        {
            // Randomly select the tile indices
            int selectedTileRow = Random.Range(1, tileCount - 1);
            int selectedTileCol = Random.Range(1, tileCount - 1);

            int adjustedTileRow = selectedTileRow - 1;
            int adjustedTileCol = selectedTileCol - 1;

            if (!doneTileLUT[adjustedTileRow, adjustedTileCol])
            {
                // Tile not yet littered, get its terrain collider (for the bounds)
                TerrainTile selectedTile = TerrainManager.Instance.GetTile(new Vector2Int(selectedTileRow, selectedTileCol));
                Bounds bounds = selectedTile.TerrainComponent.gameObject.GetComponent<TerrainCollider>().bounds;

                for (int i = 0; i < AmountToDistribute; i++)
                {
                    //Bounds bounds = DistributionArea.bounds;
                    Transform parent = transform;
                    Collectable new_collectable = SpawnRandomPrefabAtRandomPlaceInBounds(Prefabs, bounds, parent).RequireComponent<Collectable>();
                    new_collectable.Distributor = this;
                    SpawnedCollectables.Add(new_collectable);
                }

                // Update LUT and increment the counter
                doneTileLUT[adjustedTileRow, adjustedTileCol] = true;
                ++litteredTiles;
            }
        }
    }

    void Start()
    {
        StartCoroutine(CoInitialise());
    }

    public static GameObject SpawnRandomPrefabAtRandomPlaceInBounds(WeightedPrefab[] prefabs, Bounds bounds, Transform parent)
    {
        return Instantiate(
            RandomUtility.WeightedRandomInterval(prefabs, wp => wp.Weight).Prefab,
            RandomPointInBoundsOnTerrain(bounds),
            Quaternion.Euler(0f,Random.value*360f,0f), parent);
    }

    public static Vector3 RandomPointInBoundsOnTerrain(Bounds bounds)
    {
        var p = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
        p = PlaceOnTerrain(p);
        //else
        //{
        //    Debug.Log("miss!");
        //}
        return p;
    }

    public static Vector3 PlaceOnTerrain(Vector3 p)
    {
        if (Physics.Raycast(p + Vector3.up * 1000, Vector3.down, out RaycastHit hit, maxDistance: 2000,
                    layerMask: LayerMask.GetMask("Terrain")))
        {
            p.y = hit.point.y + 2.0f;
            //Debug.Log("hit!");
        }

        return p;
    }
}
