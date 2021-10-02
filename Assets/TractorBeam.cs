using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorBeam : MonoBehaviour
{
    [SerializeField] private Transform m_pullTo;
    [SerializeField] private float m_pullForce=10.0f;
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            //Debug.Log("collectable in tractor beam!");
            var pullDir = (m_pullTo.position - other.transform.position).normalized;
            other.GetComponent<Rigidbody>().AddForce(pullDir * m_pullForce);
        }

    }
}
