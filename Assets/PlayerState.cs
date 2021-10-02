using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerState : MonoBehaviour
{
    private int m_totalCollectables;

    public int Health = 3;
    public int Collected = 0;
    public float PercentageCollected => ((float)Collected) / m_totalCollectables;

    // Start is called before the first frame update
    void Start()
    {
        m_totalCollectables = FindObjectsOfType<GameObject>().Count(o=>o.tag=="Collectable");
    }
}
