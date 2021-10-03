using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightActivator : MonoBehaviour
{
    public string m_ButtonName = "Fire2";
    public Light m_Light;
    public Material[] pulsePropMaterials;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(m_ButtonName))
        {
            m_Light.enabled = !m_Light.enabled;
        }
        foreach (var pulseMaterial in pulsePropMaterials)
        {
            pulseMaterial.SetFloat("use_emissive_pulse", m_Light.enabled ? 1f : 0f);
        }
    }
}
