using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Profiling;

[RequireComponent(typeof(TerrainManager))]
public partial class OctreePathfinder : MonoBehaviour
{
    private Octree<bool>           m_PassableTree;
    private TerrainManager         m_TerrainManager;

    [SerializeField, Tooltip("Minimum size nodes will subdivide to")]
    private float                  m_MinimumNodeSize = 1f;

    [SerializeField, Tooltip("Physics layers to test for obstacles")]
    private LayerMask              m_ImpassableLayers = Physics.AllLayers;

    public static OctreePathfinder Instance { get; private set; }
    public static int              InvalidNodeID => Octree<bool>.InvalidNodeID;
    public bool                    Initialised => m_PassableTree != null;
    public int                     NodeCount => m_PassableTree.Nodes.Count;
    public IReadOnlyOctree<bool>   Octree => m_PassableTree;
    public LayerMask               ImpassableLayers => m_ImpassableLayers;

    struct AStarNode
    {
        public int NodeIndex;
        public int PrevNodeIndex;
        public float SqrDistanceToEnd;
        public float SqrDistanceFromStart;
    }

    public bool SmoothPath(Vector3 start, Vector3 end, List<Vector3> point_path, int subdivisions)
    {
        List<Vector3> unsmoothed_path = new List<Vector3>();
        if (FindPath(start, end, unsmoothed_path))
        {
            CatmullRom.SmoothPath(unsmoothed_path, point_path, subdivisions);
            return true;
        }
        return false;
    }

    public bool FindPath(Vector3 start, Vector3 end, List<Vector3> point_path)
    {
        OctreeNode<bool> start_node;
        OctreeNode<bool> end_node;

        if (FindNearestPathableNode(start, out start_node)
            && FindNearestPathableNode(end, out end_node)
            && FindPath(start_node, end_node, point_path))
        {
            point_path.Add(end);
            return true;
        }

        return false;
    }

    public bool FindPath(Vector3 start, Vector3 end, List<int> node_path)
    {
        OctreeNode<bool> start_node;
        OctreeNode<bool> end_node;

        if (FindNearestPathableNode(start, out start_node)
            && FindNearestPathableNode(end, out end_node))
        {
            return FindPath(start_node, end_node, node_path);
        }

        return false;
    }

    public bool FindPath(OctreeNode<bool> start, OctreeNode<bool> end, List<Vector3> point_path)
    {
        //Get a path in terms of octree nodes
        List<int> node_path = new List<int>();
        node_path.Add(m_PassableTree.FindNodeIndex(start));
        if (!FindPath(start, end, node_path))
        {
            return false;
        }

        ConvertNodePathToPointPath(node_path, point_path);

        return true;
    }

    public bool FindPath(OctreeNode<bool> start, OctreeNode<bool> end, List<int> node_path)
    {
        //TODO: Buffer reuse. Span<T> would make this significantly easier

        //Start and End must be passable leaf nodes
        if (!start.Data || !end.Data || !start.IsLeaf || !end.IsLeaf)
            return false;

        //Sorting predicate for the open list binary heap
        bool AstarPairLess((int, float) lhs, (int, float) rhs)
        {
            return lhs.Item2 < rhs.Item2;
        }

        int start_node_index = m_PassableTree.FindNodeIndex(start);
        int end_node_index = m_PassableTree.FindNodeIndex(end);
        Vector3 target_position = end.Bounds.center;
        List<(int, float)> open_list = new List<(int, float)>(128);
        List<int> expanded_node_ids = new List<int>(32);

        Dictionary<int, AStarNode> visited_nodes = new Dictionary<int, AStarNode>();

        //Push the start node
        visited_nodes[start_node_index] = new AStarNode { NodeIndex = start_node_index, PrevNodeIndex = InvalidNodeID, SqrDistanceFromStart = 0f, SqrDistanceToEnd = float.PositiveInfinity };
        open_list.Add((start_node_index, float.NegativeInfinity));

        int current_node_id = InvalidNodeID;
        while (!open_list.Empty())
        {
            current_node_id = open_list.HeapPop(AstarPairLess).Item1;

            //Found the end node. Exiting
            if (current_node_id == end_node_index)
            {
                break;
            }

            //Expand the node
            OctreeNode<bool> current_tree_node = m_PassableTree.Nodes[current_node_id];

            Debug.Assert(current_tree_node.IsLeaf);

            //Add Adjacent
            expanded_node_ids.Clear();
            m_PassableTree.AdjacentLeafs(current_tree_node, expanded_node_ids, (n) => n.Data);

            Vector3 current_node_center = current_tree_node.Bounds.center;
            AStarNode adjacent_search_node = new AStarNode { PrevNodeIndex = current_node_id };

            float current_distance_from_start = visited_nodes[current_node_id].SqrDistanceFromStart;
            foreach (int adjacent_node_index in expanded_node_ids)
            {
                //Update the node if this path is closer or if the node doesn't already exist
                Vector3 adjacent_node_center = m_PassableTree.Nodes[adjacent_node_index].Bounds.center;
                adjacent_search_node.SqrDistanceFromStart = current_distance_from_start + (adjacent_node_center - current_node_center).sqrMagnitude;
                adjacent_search_node.NodeIndex = adjacent_node_index;
                if (visited_nodes.TryGetValue(adjacent_node_index, out AStarNode existing_node))
                {
                    if (existing_node.SqrDistanceFromStart > adjacent_search_node.SqrDistanceFromStart)
                    {
                        //This new path is better. Update the info and add the node for re-expansion
                        adjacent_search_node.SqrDistanceToEnd = existing_node.SqrDistanceToEnd;
                        visited_nodes[adjacent_node_index] = adjacent_search_node;
                        open_list.HeapPush((adjacent_node_index, adjacent_search_node.SqrDistanceFromStart + adjacent_search_node.SqrDistanceToEnd), AstarPairLess);
                    }
                }
                else
                {
                    //Node hasn't been expanded yet. Add it
                    adjacent_search_node.SqrDistanceToEnd = (target_position - adjacent_node_center).sqrMagnitude;
                    visited_nodes[adjacent_node_index] = adjacent_search_node;
                    open_list.HeapPush((adjacent_node_index, adjacent_search_node.SqrDistanceFromStart + adjacent_search_node.SqrDistanceToEnd), AstarPairLess);
                }
            }
        }

        //Success! Found a path.
        if (current_node_id == end_node_index)
        {
            //Follow the links backwards and reuse the open_list buffer to store them in reverse order
            open_list.Clear();

            //Start will be inherently trimmed by this loop
            for (AStarNode current_search_node = visited_nodes[current_node_id]; current_search_node.PrevNodeIndex != InvalidNodeID; current_search_node = visited_nodes[current_search_node.PrevNodeIndex])
            {
                open_list.Add((current_search_node.NodeIndex, 0f));
            }

            //Now loop through the list backwards and fill the output path
            for (int i = open_list.Count - 1; i >= 0; --i)
            {
                node_path.Add(open_list[i].Item1);
            }

            //Add the end
            node_path.Add(end_node_index);

            return true;
        }

        return false;
    }

    public void ConvertNodePathToPointPath(List<int> node_path, List<Vector3> point_path)
    {
        //Convert the node path to points
        int total_nodes = node_path.Count;
        IReadOnlyList<OctreeNode<bool>> nodes = m_PassableTree.Nodes;
        Bounds[] node_bounds = new Bounds[node_path.Count];

        //Pre-cache the node bounds. Saves indexing the node list and copying the bounds, which is a double copy, each time
        for (int i = 0; i < total_nodes; ++i)
        {
            node_bounds[i] = nodes[node_path[i]].Bounds;
        }

        //Last node intentionally missed as it would go OOB
        for (int i = 0, end = node_path.Count - 1; i < end; ++i)
        {
            point_path.Add(node_bounds[i].ClosestPoint(node_bounds[i + 1].center));
        }
    }

    public bool FindNearestPathableNode(Vector3 position, out OctreeNode<bool> result_node)
    {
        //TODO: Consider buffer reuse
        //TODO: Consider using a binary minheap for the open list
        
        if (m_PassableTree.FindNearestNode(position, out result_node))
        {
            //Node is already passable. No need to expand
            if (result_node.Data)
            {
                return true;
            }

            int best_node_index = InvalidNodeID;
            float best_distance_sqr = float.PositiveInfinity;
            List<int> open_list = new List<int>(64);
            open_list.Add(0);

            while (!open_list.Empty())
            {
                int cur_node_index = open_list.PopBack();

                OctreeNode<bool> cur_node = m_PassableTree.Nodes[cur_node_index];
                //Only continue if the node is passable or contains passable children
                if (cur_node.Data)
                {
                    //If the node is (potentially) closer than our current best, expand it
                    float cur_node_distance_sqr = cur_node.Bounds.SqrDistance(position);
                    if (cur_node_distance_sqr < best_distance_sqr)
                    {
                        //If the node is a leaf. New best found!
                        if (cur_node.IsLeaf)
                        {
                            best_node_index = cur_node_index;
                            best_distance_sqr = cur_node_distance_sqr;
                        }
                        //Node is a parent. Add the children
                        else
                        {
                            for (int i = 0; i < 8; ++i)
                            {
                                open_list.Add(cur_node.ChildrenStart + i);
                            }
                        }
                    }
                }
            }

            if (best_node_index != InvalidNodeID)
            {
                result_node = m_PassableTree.Nodes[best_node_index];
                return true;
            }
        }

        result_node = m_PassableTree.Root;
        return false;
    }

    private void GeneratePassableTree()
    {
        Profiler.BeginSample("Pathinder - Generate Tree");

        Vector3 root_position = transform.position;
        Vector3 size = Vector3.one * m_TerrainManager.Definition.TerrainSize;
        Bounds tree_bounds = new Bounds(root_position + Vector3.up * (size.y * 0.5f + m_MinimumNodeSize), size);
        m_PassableTree = new Octree<bool>(tree_bounds, true);

        List<OctreeNode<bool>> open_nodes = new List<OctreeNode<bool>>(256);

        open_nodes.Add(m_PassableTree.Root);

        while (!open_nodes.Empty())
        {
            OctreeNode<bool> current_node = open_nodes.PopBack();

            Bounds current_node_bounds = current_node.Bounds;
            Vector3 current_node_extents = current_node_bounds.extents;
            Vector3 current_node_center = current_node_bounds.center;

            //Test if intersecting the terrain
            if (Physics.CheckBox(current_node_center, current_node_extents, Quaternion.identity, m_ImpassableLayers))
            {
                //Split the node if still large enough
                float child_node_size = current_node_extents.x * 0.5f;
                if (child_node_size > m_MinimumNodeSize)
                {
                    m_PassableTree.SplitNode(current_node, open_nodes);
                }
                //Flag as an impassable leaf
                else
                {
                    m_PassableTree.SetNodeData(current_node, false);
                }
            }
            //Cull if the top of the node is below the terrain
            else if (m_TerrainManager.GetTerrainElevation(current_node_center) > current_node_bounds.max.y)
            {
                m_PassableTree.SetNodeData(current_node, false);
            }
        }

        Profiler.EndSample();
    }

    private IEnumerator CoInitialise()
    {
        yield return new WaitForFixedUpdate();

        //Wait until the terrain manager is ready
        while (!Initialised && !m_TerrainManager.TerrainGenerated)
        {
            yield return new WaitForFixedUpdate();
        }

        GeneratePassableTree();
    }

#region Unity Events

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            m_TerrainManager = GetComponent<TerrainManager>();

            //Start trying to init the engine. Required because of ordering concerns
            StartCoroutine(CoInitialise());
        }
        else
        {
            Debug.LogError($"Error: More than one pathfinder exists. Objects: [{name}] and [{Instance.name}]");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

#endregion

}
