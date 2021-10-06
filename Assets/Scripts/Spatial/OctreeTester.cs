using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

class OctreeTester : MonoBehaviour
{

#if UNITY_EDITOR
    [SerializeField]
    private Transform m_PathfindTarget = null;

    [SerializeField]
    private bool m_Pathfind = false;

    [SerializeField]
    private bool m_FindNearestPathable = false;

    [SerializeField, ReadOnly]
    private Bounds m_NodeBounds = default;

    [SerializeField, ReadOnly]
    private float m_TerrainHeight = 0f;

    [SerializeField, ReadOnly]
    private float m_ClosestPointDist = float.PositiveInfinity;

    List<int> m_PathfindingResults = new List<int>();
    List<Vector3> m_PathfindingPoints = new List<Vector3>();

    private void OnDrawGizmosSelected()
    {
        OctreePathfinder pathfinder = OctreePathfinder.Instance;
        TerrainManager terrain = TerrainManager.Instance;
        if (pathfinder == null || !pathfinder.Initialised || terrain == null)
        {
            return;
        }

        //Find the bearest pathable node and draw a box around it
        if (m_FindNearestPathable)
        {
            if (pathfinder.FindNearestPathableNode(transform.position, out OctreeNode<bool> node))
            {
                m_NodeBounds = node.Bounds;
                m_TerrainHeight = terrain.GetTerrainElevation(node.Bounds.center);
                Gizmos.color = new Color(0.96f, 0.56f, 0.26f);
                Gizmos.DrawWireCube(node.Bounds.center, node.Bounds.size);
                Gizmos.color = node.Data ? Color.green : Color.red;
                Gizmos.DrawSphere(m_NodeBounds.center, m_NodeBounds.extents.x * 0.5f);
            }
        }
        //Find the closest node and draw a box around it
        else if (pathfinder.NodeAtPosition(transform.position, out OctreeNode<bool> node))
        {
            m_NodeBounds = node.Bounds;
            m_ClosestPointDist = (transform.position - m_NodeBounds.ClosestPoint(transform.position)).magnitude;
            Debug.DrawLine(transform.position, m_NodeBounds.ClosestPoint(transform.position));
            m_TerrainHeight = terrain.GetTerrainElevation(node.Bounds.center);
            Gizmos.color = new Color(0.96f, 0.56f, 0.26f);
            Gizmos.DrawWireCube(node.Bounds.center, node.Bounds.size);
        }

        if (m_Pathfind)
        {
            m_Pathfind = false;

            if (m_PathfindTarget != null)
            {
                if (pathfinder.FindPath(transform.position, m_PathfindTarget.position, m_PathfindingResults))
                {
                    Debug.Log("[Octree Tester] Path Found");
                    foreach (int i in m_PathfindingResults)
                    {
                        Bounds node_bounds = pathfinder.GetNode(i).Bounds;
                        m_PathfindingPoints.Add(node_bounds.center);
                    }
                }
                else
                {
                    Debug.Log("[Octree Tester] Path Not Found");
                }
            }
        }

        if (!m_PathfindingPoints.Empty())
        {
            Gizmos.color = Color.magenta;

            foreach (Vector3 point in m_PathfindingPoints)
            {
                Gizmos.DrawSphere(point, 0.5f);
            }
        }
    }
#endif
}
