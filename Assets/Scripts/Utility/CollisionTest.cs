using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CollisionTest : MonoBehaviour
{
    public LayerMask m_TestLayers = Physics.AllLayers;
    BoxCollider      m_Collider;

    private void Awake()
    {
        m_Collider = GetComponent<BoxCollider>();
    }

    private void OnDrawGizmosSelected()
    {
        if (m_Collider)
        {
            m_Collider.enabled = true;
            Bounds test_bounds = m_Collider.bounds;
            m_Collider.enabled = false;

            Gizmos.color = Color.green;
            if (Physics.CheckBox(transform.position, test_bounds.extents, Quaternion.identity, m_TestLayers))
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawCube(test_bounds.center, test_bounds.size);
        }
    }
}
