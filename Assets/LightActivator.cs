using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightActivator : MonoBehaviour
{
    public string m_ButtonName = "Fire2";
    public Light m_Light;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(m_ButtonName))
        {
            m_Light.enabled = !m_Light.enabled;
        }
    }
}
