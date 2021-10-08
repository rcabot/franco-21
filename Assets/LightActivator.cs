using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightActivator : MonoBehaviour
{
    private readonly int emissive_pulse_id = Shader.PropertyToID("use_emissive_pulse");
    private bool m_LightsEnabled = true;
    public string m_ButtonName = "Fire2";
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

        foreach (var pulseMaterial in pulsePropMaterials)
        {
            pulseMaterial.SetFloat(emissive_pulse_id, toggle ? 1f : 0f);
        }

        OnLightsToggled?.Invoke(toggle);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Locked && Input.GetButtonDown(m_ButtonName))
        {
            if (m_Lights.Length >= 0)
            {
                ToggleLights(!m_LightsEnabled);
            }
        }
    }
}
