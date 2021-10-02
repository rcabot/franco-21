using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectablesDistributor : MonoBehaviour
{
    public BoxCollider DistributionArea;
    public int AmountToDistribute;
    public GameObject[] Prefabs;
    public List<GameObject> SpawnedCollectables;
    private bool spawned;

    void Update()
    {
        if (!spawned)
        {
            var prefabsLength = Prefabs.Length;
            for (int i = 0; i < AmountToDistribute; i++)
            {
                GameObject[] prefabs = Prefabs;
                Bounds bounds = DistributionArea.bounds;
                Transform parent = transform;
                SpawnedCollectables.Add(SpawnRandomPrefabAtRandomPlaceInBounds(prefabsLength, prefabs, bounds, parent));
            }
            spawned = true;
        }
    }

    public static GameObject SpawnRandomPrefabAtRandomPlaceInBounds(int prefabsLength, GameObject[] prefabs, Bounds bounds, Transform parent)
    {
        return Instantiate(
            prefabs[Random.Range(0, prefabsLength)], 
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
        var hits = Physics.RaycastAll(p + Vector3.up * 1000, Vector3.down, maxDistance: 2000,
            layerMask: LayerMask.GetMask("Terrain"));
        if (hits.Any())
        {
            p.y = hits.First().point.y;
            Debug.Log("hit!");
        }
        else
        {
            Debug.Log("miss!");
        }
        return p;
    }
}
