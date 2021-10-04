using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorBeamActivator : MonoBehaviour
{
    [SerializeField] private GameObject m_tractorBeam = null;

    public bool TractorActive { get; private set; }
    AudioSource tractorAudio;
    private SubmarineController m_PlayerSubmarine;

    private void Start()
    {
        tractorAudio = GetComponent<AudioSource>();
        m_PlayerSubmarine = FindObjectOfType<SubmarineController>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isOn = m_PlayerSubmarine.LightsOn;
        TractorActive = isOn ? Input.GetButton("Fire1") : false;
        m_tractorBeam.SetActive(TractorActive);
        if( TractorActive && tractorAudio.isPlaying == false )
        {
            tractorAudio.Play();
        }
        else if( TractorActive == false )
        {
            tractorAudio.Stop();
        }
    }
}
