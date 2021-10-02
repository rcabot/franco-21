using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorBeam : MonoBehaviour
{

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            Debug.Log("collectable in tractor beam!");
        }

    }
}
