using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorBeamActivator : MonoBehaviour
{
    [SerializeField] private GameObject m_tractorBeam = null;

    public bool TractorActive { get; private set; }

    // Update is called once per frame
    void Update()
    {
        TractorActive = Input.GetButton("Fire1");
        m_tractorBeam.SetActive(TractorActive);
    }
}
