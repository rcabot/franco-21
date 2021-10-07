#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public partial class HunterBehaviour : MonoBehaviour
{
    public static bool EnableLogging = false;

    public void DebugTeleport(Vector3 position)
    {
        LeaveLimbo(position);
    }

    private void OnValidate()
    {
        float max_friction = m_FrictionCurve.keys.Max(k => k.value);

        if (m_BackstageSettings.Acceleration > max_friction)
        {
            Debug.LogWarning("Warning: Creature backstage state has higher acceleration than maximum friction. Speed is infinite!", this);
        }

        if (m_FrontstageIdleSettings.Acceleration > max_friction)
        {
            Debug.LogWarning("Warning: Creature Fronstage Idle state has higher acceleration than maximum friction. Speed is infinite!", this);
        }

        if (m_FrontstageDistantSettings.Acceleration > max_friction)
        {
            Debug.LogWarning("Warning: Creature Fronstage Distant state has higher acceleration than maximum friction. Speed is infinite!", this);
        }

        if (m_FrontstageSuspiciousSettings.Acceleration > max_friction)
        {
            Debug.LogWarning("Warning: Creature Fronstage Suspicious state has higher acceleration than maximum friction. Speed is infinite!", this);
        }

        if (m_FrontstageCloseSettings.Acceleration > max_friction)
        {
            Debug.LogWarning("Warning: Creature Fronstage Close state has higher acceleration than maximum friction. Speed is infinite!", this);
        }

        if (m_AttackingSettings.Acceleration > max_friction)
        {
            Debug.LogWarning("Warning: Creature Attacking state has higher acceleration than maximum friction. Speed is infinite!", this);
        }

        if (m_RetreatSettings.Acceleration > max_friction)
        {
            Debug.LogWarning("Warning: Creature Retreat state has higher acceleration than maximum friction. Speed is infinite!", this);
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 1, end = m_Path.Count - 1; i < end; ++i)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(m_Path[i], m_Path[i + 1]);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_Path[i], m_PathNodeDistance);
        }

        if (!m_Path.Empty())
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, m_Path.Front());

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_Path.Front(), m_PathNodeDistance);
            Gizmos.DrawSphere(m_Path.Back(), m_PathNodeDistance);
        }
    }
}

#endif