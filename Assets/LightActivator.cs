using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightActivator : MonoBehaviour
{
    private readonly int emissive_pulse_id = Shader.PropertyToID("use_emissive_pulse");
    public string m_ButtonName = "Fire2";
    public Light[] m_Lights;
    public Material[] pulsePropMaterials;

    public bool LightsEnabled => m_Lights.Length > 0 && m_Lights[0].enabled;

    public void ToggleLights(bool toggle)
    {
        bool lights_enabled = !m_Lights[0].enabled;
        foreach (Light l in m_Lights)
        {
            l.enabled = lights_enabled;
        }

        foreach (var pulseMaterial in pulsePropMaterials)
        {
            pulseMaterial.SetFloat(emissive_pulse_id, lights_enabled ? 1f : 0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(m_ButtonName)) 
        {
            if (m_Lights.Length >= 0)
            {
                ToggleLights(!m_Lights[0].enabled);
            }
        }
    }
}
