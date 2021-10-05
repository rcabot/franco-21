#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

public partial class OctreePathfinder : MonoBehaviour
{
    [Flags]
    public enum DebugNodeDrawFlags
    {
        None = 0,
        DrawLeafOnly = 1 << 0,
        DrawPassable = 1 << 1,
        DrawImpassable = 1 << 2,
        DrawOuterBounds = 1 << 3,
        DrawLastSplitBounds = 1 << 4
    }

    [NonSerialized]
    public DebugNodeDrawFlags Debug_ActiveNodeDrawFlags = DebugNodeDrawFlags.DrawLeafOnly | DebugNodeDrawFlags.DrawImpassable;

    [NonSerialized]
    public int Debug_HighlightNodeIndex = -1;

    public OctreeNode<bool> Debug_HighlightNode => Debug_HighlightNodeIndex >= 0 && Debug_HighlightNodeIndex < NodeCount ? m_PassableTree.Nodes[Debug_HighlightNodeIndex] : default;

    [NonSerialized]
    public int BreakIndex = -1;

    private List<Bounds> Debug_LastSplitBounds = new List<Bounds>();

    public void DebugRegeneratePassableTree()
    {
        m_PassableTree.Clear();
        GeneratePassableTree();
    }

    public void DebugSplitTreeNodes()
    {
        //Splitting the nodes modifies the nodes list. Cache the end index
        int nodes_to_divide = m_PassableTree.Nodes.Count;

        Debug_LastSplitBounds.Clear();
        OctreeNode<bool>[] new_children = new OctreeNode<bool>[8];
        for (int i = 0; i < nodes_to_divide; ++i)
        {
            if (m_PassableTree.SplitNode(i, new_children))
            {
                foreach (OctreeNode<bool> child in new_children)
                {
                    Debug_LastSplitBounds.Add(child.Bounds);
                    if (Physics.CheckBox(child.Bounds.center, child.Bounds.extents, Quaternion.identity, m_ImpassableLayers))
                    {
                        m_PassableTree.SetNodeData(child, false);
                    }
                }
            }
        }
    }

    public void DebugClearTree()
    {
        m_PassableTree.Clear();
    }

    private void OnDrawGizmos()
    {
        if (Initialised)
        {
            if (Debug_ActiveNodeDrawFlags.HasFlag(DebugNodeDrawFlags.DrawOuterBounds))
            {
                Bounds root_bounds = m_PassableTree.Root.Bounds;
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireCube(root_bounds.center, root_bounds.size);
            }

            bool draw_leaf_only = Debug_ActiveNodeDrawFlags.HasFlag(DebugNodeDrawFlags.DrawLeafOnly);
            if (Debug_ActiveNodeDrawFlags.HasFlag(DebugNodeDrawFlags.DrawImpassable))
            {
                Gizmos.color = Color.red;
                foreach (OctreeNode<bool> node in m_PassableTree.Nodes)
                {
                    if (!node.Data && (!draw_leaf_only || node.IsLeaf))
                    {
                        Gizmos.DrawWireCube(node.Bounds.center, node.Bounds.size);
                    }
                }
            }

            if (Debug_ActiveNodeDrawFlags.HasFlag(DebugNodeDrawFlags.DrawPassable))
            {
                Gizmos.color = Color.green;
                foreach (OctreeNode<bool> node in m_PassableTree.Nodes)
                {
                    if (node.Data && (!draw_leaf_only || node.IsLeaf))
                    {
                        Gizmos.DrawWireCube(node.Bounds.center, node.Bounds.size);
                    }
                }
            }

            if (Debug_ActiveNodeDrawFlags.HasFlag(DebugNodeDrawFlags.DrawLastSplitBounds))
            {
                Gizmos.color = Color.cyan;
                foreach (Bounds b in Debug_LastSplitBounds)
                {
                    Gizmos.DrawWireCube(b.center, b.size);
                }
            }

            if (Debug_HighlightNodeIndex >= 0 && Debug_HighlightNodeIndex < m_PassableTree.Nodes.Count)
            {
                OctreeNode<bool> node = m_PassableTree.Nodes[Debug_HighlightNodeIndex];
                Gizmos.color = Physics.CheckBox(node.Bounds.center, node.Bounds.extents, Quaternion.identity, m_ImpassableLayers) ? Color.red : Color.green;
                Gizmos.DrawCube(node.Bounds.center, node.Bounds.size);
            }
        }
    }
}

#endif //UNITY_EDITOR