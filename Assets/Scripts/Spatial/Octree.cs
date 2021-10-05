using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

public enum Octant : byte
{
    TopNE = 0,
    TopNW,
    TopSE,
    TopSW,
    BottomNW,
    BottomNE,
    BottomSE,
    BottomSW,

    INVALID
}

public struct OctreeNode<T>
{
    public const int InvalidNodeID = -1;
    public int       Parent;
    public int       ChildrenStart;
    public Bounds    Bounds;
    public Octant    Octant;
    public T         Data;

    public bool HasChildren => ChildrenStart != InvalidNodeID;

    public OctreeNode(int parent, Bounds bounds, Octant octant, T data)
    {
        Parent = parent;
        ChildrenStart = InvalidNodeID;
        Bounds = bounds;
        Octant = octant;
        Data = data;
    }
}

public class Octree<T>
{
    List<OctreeNode<T>> m_Nodes = new List<OctreeNode<T>>();
    T                   m_InitialValue = default;

    public const int                    InvalidNodeID = OctreeNode<T>.InvalidNodeID;
    public IReadOnlyList<OctreeNode<T>> Nodes => m_Nodes;
    public OctreeNode<T>                Root => m_Nodes[0];
    public Bounds                       WorldBounds => Root.Bounds;
    
    //Set the amount of nodes that the internal list has capacity for. Will not go below size
    public int                          NodeCapacity
    {
        get => m_Nodes.Capacity;
        set
        {
            m_Nodes.Capacity = Mathf.Max(value, m_Nodes.Count);
        }
    }

    public Octree(Bounds world_bounds, T initial_value)
    {
        m_Nodes.Add(new OctreeNode<T>(InvalidNodeID, world_bounds, Octant.INVALID, initial_value));
        m_InitialValue = initial_value;
    }

    public int FindNodeIndex(OctreeNode<T> node)
    {
        if (node.Parent != InvalidNodeID)
        {
            return m_Nodes[node.Parent].ChildrenStart + (int)node.Octant;
        }

        //Either root or invalid
        return node.Octant == Octant.INVALID ? 0 : InvalidNodeID;
    }

    public bool SplitNode(OctreeNode<T> node, OctreeNode<T>[] new_children)
    {
        return SplitNode(FindNodeIndex(node), new_children);
    }

    public bool SplitNode(int node_id, OctreeNode<T>[] new_children)
    {
        OctreeNode<T> parent = m_Nodes[node_id];
        if (parent.ChildrenStart == InvalidNodeID)
        {
            Bounds parent_bounds = parent.Bounds;
            for (int i = 0; i < 8; ++i)
            {
                Octant child_octant = (Octant)i;
                OctreeNode<T> child = new OctreeNode<T>(node_id, GetChildBounds(parent_bounds, child_octant), child_octant, m_InitialValue);
                new_children[i] = child;
            }

            parent.ChildrenStart = m_Nodes.Count;
            m_Nodes.AddRange(new_children);
            m_Nodes[node_id] = parent;

            return true;
        }

        return false;
    }

    public bool SplitNodeAndAppend(OctreeNode<T> node, IList<OctreeNode<T>> result_list)
    {
        return SplitNodeAndAppend(FindNodeIndex(node), result_list);
    }

    public bool SplitNodeAndAppend(int node_id, IList<OctreeNode<T>> result_list)
    {
        OctreeNode<T> parent = m_Nodes[node_id];
        if (parent.ChildrenStart == InvalidNodeID)
        {
            parent.ChildrenStart = m_Nodes.Count;

            Bounds parent_bounds = parent.Bounds;
            for (int i = 0; i < 8; ++i)
            {
                Octant child_octant = (Octant)i;
                OctreeNode<T> child = new OctreeNode<T>(node_id, GetChildBounds(parent_bounds, child_octant), child_octant, m_InitialValue);
                result_list.Add(child);
                m_Nodes.Add(child);
            }
            m_Nodes[node_id] = parent;

            return true;
        }

        return false;
    }

    public void SetNodeData(OctreeNode<T> node, T data)
    {
        SetNodeData(FindNodeIndex(node), data);
    }

    public void SetNodeData(int node_id, T data)
    {
        OctreeNode<T> node = m_Nodes[node_id];
        node.Data = data;
        m_Nodes[node_id] = node;
    }

    public void Clear()
    {
        OctreeNode<T> root = Root;
        root.ChildrenStart = InvalidNodeID;

        m_Nodes.Clear();
        m_Nodes.Add(root);
    }

    public void ShrinkToFit()
    {
        m_Nodes.Capacity = m_Nodes.Count;
    }

    public static Bounds GetChildBounds(Bounds parent_bounds, Octant child_octant)
    {
        Vector3 child_size = parent_bounds.extents; //Half of the size in all dimensions
        Vector3 child_position = parent_bounds.center;
        Vector3 child_extents = child_size * 0.5f;

        //Offset the bounds center based on the octant
        switch (child_octant)
        {
            case Octant.TopNE:
                child_position += child_extents;
                break;
            case Octant.TopNW:
                child_position.x -= child_extents.x;
                child_position.y += child_extents.y;
                child_position.z += child_extents.z;
                break;
            case Octant.TopSE:
                child_position.x += child_extents.x;
                child_position.y += child_extents.y;
                child_position.z -= child_extents.z;
                break;
            case Octant.TopSW:
                child_position.x -= child_extents.x;
                child_position.y += child_extents.y;
                child_position.z -= child_extents.z;
                break;
            case Octant.BottomNW:
                child_position.x -= child_extents.x;
                child_position.y -= child_extents.y;
                child_position.z += child_extents.z;
                break;
            case Octant.BottomNE:
                child_position.x += child_extents.x;
                child_position.y -= child_extents.y;
                child_position.z += child_extents.z;
                break;
            case Octant.BottomSE:
                child_position.x += child_extents.x;
                child_position.y -= child_extents.y;
                child_position.z -= child_extents.z;
                break;
            case Octant.BottomSW:
            default:
                child_position -= child_extents;
                break;
        }

        return new Bounds(child_position, child_size);
    }
}
