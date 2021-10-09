using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LightActivator : MonoBehaviour
{
    private readonly int emissive_pulse_id = Shader.PropertyToID("use_emissive_pulse");
    private bool m_LightsEnabled = true;
    public InputActionReference m_ButtonAction;
    public Light[] m_Lights;
    public Material[] pulsePropMaterials;

    public event Action<bool> OnLightsToggled;
    public bool LightsEnabled => m_LightsEnabled;
    public bool Locked = false;

    public void ToggleLights(bool toggle)
    {
        m_LightsEnabled = toggle;

        foreach (Light l in m_Lights)
        {
            l.enabled = toggle;
        }

        SetupEmissiveMaterials(toggle);
        OnLightsToggled?.Invoke(toggle);
    }

    private void SetupEmissiveMaterials(bool on)
    {
        foreach (var pulseMaterial in pulsePropMaterials)
        {
            pulseMaterial.SetFloat(emissive_pulse_id, on ? 1f : 0f);
        }
    }

    private void OnLightTogglePressed(InputAction.CallbackContext obj)
    {
        if (!Locked)
        {
            if (m_Lights.Length >= 0)
            {
                ToggleLights(!m_LightsEnabled);
            }
        }
    }

    private void Start()
    {
        foreach (Light l in m_Lights)
        {
            l.enabled = m_LightsEnabled;
        }

        SetupEmissiveMaterials(m_LightsEnabled);

        m_ButtonAction.action.performed += OnLightTogglePressed;
    }

    private void OnDestroy()
    {
        m_ButtonAction.action.performed -= OnLightTogglePressed;
    }

}
