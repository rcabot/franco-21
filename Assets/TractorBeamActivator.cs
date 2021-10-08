using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TractorBeamActivator : MonoBehaviour
{
    [SerializeField] private GameObject m_tractorBeam = null;
    [SerializeField] private InputActionReference m_tractorAction;

    public bool TractorActive { get; private set; }
    AudioSource tractorAudio;
    private SubmarineController m_PlayerSubmarine;

    private void OnTractorPressStart(InputAction.CallbackContext obj)
    {
        TractorActive = m_PlayerSubmarine.LightsOn;
    }

    private void OnTractorPressCanceled(InputAction.CallbackContext obj)
    {
        TractorActive = false;
    }

    private void Start()
    {
        tractorAudio = GetComponent<AudioSource>();
        m_PlayerSubmarine = FindObjectOfType<SubmarineController>();

        m_tractorAction.action.started += OnTractorPressStart;
        m_tractorAction.action.canceled += OnTractorPressCanceled;
    }

    // Update is called once per frame
    void Update()
    {
        bool isOn = m_PlayerSubmarine.LightsOn;
        TractorActive = TractorActive && isOn;
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

    private void OnDestroy()
    {
        m_tractorAction.action.started -= OnTractorPressStart;
        m_tractorAction.action.canceled -= OnTractorPressCanceled;
    }
}
