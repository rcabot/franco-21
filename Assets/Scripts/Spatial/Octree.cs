﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum OctreeDirection
{
    North,
    South,
    East,
    West,
    Up,
    Down
}

public enum Octant : byte
{
    TopNE = 0,
    TopNW,
    TopSE,
    TopSW,
    BottomNE,
    BottomNW,
    BottomSE,
    BottomSW,

    INVALID
}

//Helpers to get adjacent octants. All cardinal directions are paired due to the repeating nature of the grid
public static class OctantExtensions
{
    public static Octant OppositeX(this Octant o)
    {
        switch (o)
        {
            case Octant.TopNE:
                return Octant.TopNW;
            case Octant.TopNW:
                return Octant.TopNE;
            case Octant.TopSE:
                return Octant.TopSW;
            case Octant.TopSW:
                return Octant.TopSE;
            case Octant.BottomNE:
                return Octant.BottomNW;
            case Octant.BottomNW:
                return Octant.BottomNE;
            case Octant.BottomSE:
                return Octant.BottomSW;
            case Octant.BottomSW:
                return Octant.BottomSE;
            case Octant.INVALID:
            default:
                return Octant.INVALID;
        }
    }

    public static Octant OppositeY(this Octant o)
    {
        switch (o)
        {
            case Octant.TopNE:
                return Octant.BottomNE;
            case Octant.TopNW:
                return Octant.BottomNW;
            case Octant.TopSE:
                return Octant.BottomSE;
            case Octant.TopSW:
                return Octant.BottomSW;
            case Octant.BottomNE:
                return Octant.TopNE;
            case Octant.BottomNW:
                return Octant.TopNW;
            case Octant.BottomSE:
                return Octant.TopSE;
            case Octant.BottomSW:
                return Octant.TopSW;
            case Octant.INVALID:
            default:
                return Octant.INVALID;
        }
    }

    public static Octant OppositeZ(this Octant o)
    {
        switch (o)
        {
            case Octant.TopNE:
                return Octant.TopSE;
            case Octant.TopNW:
                return Octant.TopSW;
            case Octant.TopSE:
                return Octant.TopNE;
            case Octant.TopSW:
                return Octant.TopNW;
            case Octant.BottomNE:
                return Octant.BottomSE;
            case Octant.BottomNW:
                return Octant.BottomSW;
            case Octant.BottomSE:
                return Octant.BottomNE;
            case Octant.BottomSW:
                return Octant.BottomNW;
            case Octant.INVALID:
            default:
                return Octant.INVALID;
        }
    }

    public static Octant MoveDirection(this Octant o, OctreeDirection direction)
    {
        switch (direction)
        {
            case OctreeDirection.North:
            case OctreeDirection.South:
                return OppositeZ(o);
            case OctreeDirection.East:
            case OctreeDirection.West:
                return OppositeX(o);
            case OctreeDirection.Up:
            case OctreeDirection.Down:
                return OppositeY(o);
            default:
                return Octant.INVALID;
        }
    }

    public static bool IsNorth(this Octant o)
    {
        switch (o)
        {
            case Octant.TopNE:
            case Octant.TopNW:
            case Octant.BottomNE:
            case Octant.BottomNW:
                return true;
            default:
                return false;
        }
    }

    public static bool IsSouth(this Octant o)
    {
        switch (o)
        {
            case Octant.TopSE:
            case Octant.TopSW:
            case Octant.BottomSE:
            case Octant.BottomSW:
                return true;
            default:
                return false;
        }
    }

    public static bool IsEast(this Octant o)
    {
        switch (o)
        {
            case Octant.TopNE:
            case Octant.TopSE:
            case Octant.BottomNE:
            case Octant.BottomSE:
                return true;
            default:
                return false;
        }
    }

    public static bool IsWest(this Octant o)
    {
        switch (o)
        {
            case Octant.TopNW:
            case Octant.TopSW:
            case Octant.BottomNW:
            case Octant.BottomSW:
                return true;
            default:
                return false;
        }
    }

    public static bool IsTop(this Octant o)
    {
        switch (o)
        {
            case Octant.TopNE:
            case Octant.TopNW:
            case Octant.TopSE:
            case Octant.TopSW:
                return true;
            default:
                return false;
        }
    }

    public static bool IsBottom(this Octant o)
    {
        switch (o)
        {
            case Octant.BottomNE:
            case Octant.BottomNW:
            case Octant.BottomSE:
            case Octant.BottomSW:
                return true;
            default:
                return false;
        }
    }

    public static bool DirectionCrossesNode(this Octant o, OctreeDirection direction)
    {
        switch (direction)
        {
            case OctreeDirection.North:
                return o.IsNorth();
            case OctreeDirection.South:
                return o.IsSouth();
            case OctreeDirection.East:
                return o.IsEast();
            case OctreeDirection.West:
                return o.IsWest();
            case OctreeDirection.Up:
                return o.IsTop();
            case OctreeDirection.Down:
                return o.IsBottom();
            default:
                return false;
        }
    }
}

public struct OctreeNode<T>
{
    public const int InvalidNodeID = -1;
    public int       Parent;
    public int       ChildrenStart;
    public Bounds    Bounds;
    public Octant    Octant;
    public T         Data;

    public bool      IsLeaf => ChildrenStart == InvalidNodeID;

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

    public int GetAdjacentNode(OctreeNode<T> node, OctreeDirection direction)
    {
        if (node.Parent != InvalidNodeID)
        {
            Octant node_octant = node.Octant;
            Octant target_octant = node_octant.MoveDirection(direction);

            OctreeNode<T> parent = Nodes[node.Parent];
            //Opposite nodes are contained in the same parent. Simply return it
            if (!node_octant.DirectionCrossesNode(direction))
            {
                return parent.ChildrenStart + (int)target_octant;
            }
            //Get the adjacent node at the parent level and get the adjacent node to this from there
            else
            {
                int parent_adjacent_id = GetAdjacentNode(parent, direction);
                if (parent_adjacent_id != InvalidNodeID)
                {
                    //Now that we have the parent node. Test for children in the target octant. Return the parent adjacent if there are no children
                    OctreeNode<T> parent_adjacent = Nodes[parent_adjacent_id];
                    return parent_adjacent.IsLeaf ? parent_adjacent_id : parent_adjacent.ChildrenStart + (int)target_octant;
                }
            }
        }

        return InvalidNodeID;
    }

    public bool GetAdjacentNodes(OctreeNode<T> node, int[] adjacent_node_ids)
    {
        //Up, Down, West, East, North, South
        if (node.Parent != InvalidNodeID)
        {
            //It's a cube. I'm assuming 6 adjacents...
            for (int i = 0; i < 6; ++i)
            {
                adjacent_node_ids[i] = GetAdjacentNode(node, (OctreeDirection)i);
            }
            return true;
        }

        return false;
    }

    public bool SplitNode(OctreeNode<T> node, OctreeNode<T>[] new_children)
    {
        return SplitNode(FindNodeIndex(node), new_children);
    }

    public bool SplitNode(int node_id, OctreeNode<T>[] new_children)
    {
        OctreeNode<T> parent = m_Nodes[node_id];
        if (parent.IsLeaf)
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
        if (parent.IsLeaf)
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
