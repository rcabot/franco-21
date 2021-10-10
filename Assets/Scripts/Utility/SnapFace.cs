using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapFace : MonoBehaviour
{
    Transform m_Target;

    private void Start()
    {
        SubmarineController sub = FindObjectOfType<SubmarineController>();
        if (sub == null)
        {
            Destroy(this);
            Debug.LogError("Error: Snap Face object has no target", this);
        }
        else
        {
            m_Target = sub.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation((m_Target.position - transform.position).normalized);
    }
}
