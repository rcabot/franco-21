using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorBeamActivator : MonoBehaviour
{
    [SerializeField] private GameObject m_tractorBeam = null;

    // Update is called once per frame
    void Update()
    {
        m_tractorBeam.SetActive(Input.GetButton("Fire1"));
    }
}
