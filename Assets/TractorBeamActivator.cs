using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TractorBeamActivator : MonoBehaviour
{
    [SerializeField] private GameObject m_tractorBeam = null;

    public bool TractorActive { get; private set; }


    AudioSource tractorAudio;

    private void Start()
    {
        tractorAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        TractorActive = Input.GetButton("Fire1");
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
