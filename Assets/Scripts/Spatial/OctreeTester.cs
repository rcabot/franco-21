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

    [SerializeField, ReadOnly]
    private int m_CurrentNodeID = OctreeNode<bool>.InvalidNodeID;

    List<int> m_PathfindingResults = new List<int>();
    List<Vector4> m_PathfindingPoints = new List<Vector4>();

    [Flags]
    enum OctreeTesterFlags
    {
        None = 0,
        ExpandAdjacent = 1 << 1,
        ExpandAdjacentPathable = 1 << 2,
        ExpandAdjacentAll = 1 << 3,
        Pathfind = 1 << 4,
        ShowVisitedNodes = 1 << 5
    }

    [SerializeField]
    private OctreeTesterFlags m_Flags = OctreeTesterFlags.None;

    [SerializeField]
    private OctreeDirection m_ExpansionDirection = OctreeDirection.Down;

    private void OnDrawGizmosSelected()
    {
        OctreePathfinder pathfinder = OctreePathfinder.Instance;
        TerrainManager terrain = TerrainManager.Instance;
        if (pathfinder == null || !pathfinder.Initialised || terrain == null)
        {
            return;
        }

        OctreeNode<bool>? current_node = null;

        //Find the bearest pathable node and draw a box around it
        if (m_FindNearestPathable)
        {
            if (pathfinder.FindNearestPathableNode(transform.position, out OctreeNode<bool> node))
            {
                current_node = node;
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
            current_node = node;
            m_NodeBounds = node.Bounds;
            m_ClosestPointDist = (transform.position - m_NodeBounds.ClosestPoint(transform.position)).magnitude;
            Debug.DrawLine(transform.position, m_NodeBounds.ClosestPoint(transform.position));
            m_TerrainHeight = terrain.GetTerrainElevation(node.Bounds.center);
            Gizmos.color = new Color(0.96f, 0.56f, 0.26f);
            Gizmos.DrawWireCube(node.Bounds.center, node.Bounds.size);
        }

        m_CurrentNodeID = current_node.HasValue ? pathfinder.Octree.FindNodeIndex(current_node.Value) : OctreeNode<bool>.InvalidNodeID;

        if (m_Flags.HasFlag(OctreeTesterFlags.ExpandAdjacent) && current_node != null)
        {
            List<int> adjacent_leafs = new List<int>();

            if (m_Flags.HasFlag(OctreeTesterFlags.ExpandAdjacentAll))
            {
                pathfinder.Octree.AdjacentLeafs(current_node.Value, adjacent_leafs, n => n.Data || !m_Flags.HasFlag(OctreeTesterFlags.ExpandAdjacentPathable));
            }
            else
            {
                pathfinder.Octree.AdjacentLeafs(current_node.Value, m_ExpansionDirection, adjacent_leafs, n => n.Data || !m_Flags.HasFlag(OctreeTesterFlags.ExpandAdjacentPathable));
            }

            foreach (int adjacent in adjacent_leafs)
            {
                OctreeNode<bool> node = pathfinder.Octree.Nodes[adjacent];
                Bounds b = node.Bounds;
                Gizmos.color = node.Data ? Color.green : Color.red;
                Gizmos.DrawSphere(b.center, b.extents.x * 0.9f);
            }
        }

        if (m_Pathfind)
        {
            //m_Pathfind = false;

            if (m_PathfindTarget != null)
            {
                if (pathfinder.FindPath(transform.position, m_PathfindTarget.position, m_PathfindingResults))
                {
                    Debug.Log("[Octree Tester] Path Found");
                    m_PathfindingPoints.Clear();
                    foreach (int i in m_PathfindingResults)
                    {
                        Bounds node_bounds = pathfinder.Octree.Nodes[i].Bounds;
                        m_PathfindingPoints.Add(node_bounds.center.ToVector4(node_bounds.extents.x * 0.9f));
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
            foreach (Vector4 point in m_PathfindingPoints)
            {
                Gizmos.DrawSphere(point, point.w);
            }
        }
        
        if (m_Flags.HasFlag(OctreeTesterFlags.ShowVisitedNodes))
        {
            Gizmos.color = Color.yellow;
            foreach (int node_id in pathfinder.visited_nodes.Keys)
            {
                Bounds node_bounds = pathfinder.Octree.Nodes[node_id].Bounds;
                Gizmos.DrawWireCube(node_bounds.center, node_bounds.size);
            }
        }

        if (m_PathfindTarget)
        {
            if (pathfinder.FindNearestPathableNode(m_PathfindTarget.position, out OctreeNode<bool> node))
            {
                Gizmos.color = new Color(0.96f, 0.56f, 0.26f);
                Gizmos.DrawWireCube(node.Bounds.center, node.Bounds.size);
                Gizmos.DrawSphere(node.Bounds.center, node.Bounds.extents.x * 0.9f);
            }
        }
    }
#endif
}
