using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectablesDistributor : MonoBehaviour
{
    public BoxCollider DistributionArea;
    public int AmountToDistribute;
    public GameObject[] Prefabs;
    public List<GameObject> SpawnedCollectables;
    // Start is called before the first frame update
    void Awake()
    {
        var prefabsLength = Prefabs.Length;
        for (int i = 0; i < AmountToDistribute; i++)
        {
            GameObject[] prefabs = Prefabs;
            Bounds bounds = DistributionArea.bounds;
            Transform parent = transform;
            SpawnedCollectables.Add(SpawnRandomPrefabAtRandomPlaceInBounds(prefabsLength, prefabs, bounds, parent));
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
        if (Physics.Raycast(p + Vector3.up*1000,Vector3.down,maxDistance:2000,hitInfo: out var hit))
        {
            p.y = hit.point.y;
        }
        return p;
    }
}
