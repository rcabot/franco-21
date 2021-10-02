using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectablesDistributor : MonoBehaviour
{
    public BoxCollider DistributionArea;
    public int AmountToDistribute;
    public GameObject[] Prefabs;
    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < AmountToDistribute; i++)
        {
            Instantiate(Prefabs[0], RandomPointInBounds(DistributionArea.bounds), 
                Quaternion.identity, transform);
        }
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
