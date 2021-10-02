using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityRandom = UnityEngine.Random;

public class ShakeableObject : MonoBehaviour
{
    private Vector2 m_original_position = Vector2.zero;
    private Vector2 m_ShakeOffset = Vector2.zero;

    public void Shake(float magnitude)
    {
        if (m_original_position.Approximately(Vector2.zero))
        {
            m_original_position = transform.position;
        }

        Vector2 offset = UnityRandom.insideUnitCircle * magnitude;
        transform.Translate(offset - m_ShakeOffset, Space.Self);
        m_ShakeOffset = offset;
    }

    public void StopShake()
    {
        transform.Translate(-m_ShakeOffset, Space.Self);
        m_ShakeOffset = Vector2.zero;
        m_original_position = Vector3.zero;
    }

    private void Start()
    {
        WorldShakeManager.AddShakeable(this);
    }

    private void OnDestroy()
    {
        WorldShakeManager.RemoveShakeable(this);
    }
}
