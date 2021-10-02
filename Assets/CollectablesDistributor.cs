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
            RandomPointInBounds(bounds),
            Quaternion.identity, parent);
    }

    public static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
