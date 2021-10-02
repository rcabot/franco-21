using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightActivator : MonoBehaviour
{
    public string ButtonName = "Fire2";
    public Light light;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown(ButtonName))
        {
            light.enabled = !light.enabled;
        }
    }
}
