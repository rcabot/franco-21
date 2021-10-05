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

    public bool FindNearestPathableNode(Vector3 position, out OctreeNode<bool> result_node)
    {
        //TODO: Consider buffer reuse
        
        if (NodeAtPosition(position, out result_node))
        {
            //Node is already passable. No need to expand
            if (result_node.Data)
            {
                return true;
            }

            int best_node_index = InvalidNodeID;
            float best_distance_sqr = float.PositiveInfinity;
            HashSet<int> visited_nodes = new HashSet<int>();
            List<int> open_list = new List<int>(64);
            open_list.Add(0);

            while (!open_list.Empty())
            {
                int cur_node_index = open_list.PopBack();
                visited_nodes.Add(cur_node_index);

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

    public bool NodeAtPosition(Vector3 position, out OctreeNode<bool> result_node)
    {
        //I don't want to expose the octree. I would if C# had const access as a concept.
        return m_PassableTree.NodeAtPosition(position, out result_node);
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
            int node_index = m_PassableTree.FindNodeIndex(current_node);

#if UNITY_EDITOR
            if (node_index == BreakIndex)
            {
                Debug.DebugBreak();
            }
#endif

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
                    m_PassableTree.SplitNodeAndAppend(current_node, open_nodes);
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
