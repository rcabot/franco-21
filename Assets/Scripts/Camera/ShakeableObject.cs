using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityRandom = UnityEngine.Random;

public class ShakeableObject : MonoBehaviour
{
    private Vector3 m_ShakeOffset = Vector3.zero;

    public void Shake(float magnitude)
    {
        Vector3 offset = UnityRandom.insideUnitCircle * magnitude;
        transform.Translate(-m_ShakeOffset + offset, Space.Self);
        m_ShakeOffset = offset;
    }

    public void StopShake()
    {
        transform.Translate(-m_ShakeOffset, Space.Self);
    }

    private void Awake()
    {
        WorldShakeManager.AddShakeable(this);
    }

    private void OnDestroy()
    {
        WorldShakeManager.RemoveShakeable(this);
    }
}
