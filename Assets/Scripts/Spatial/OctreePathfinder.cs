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
    private Octree<bool>     m_PassableTree;
    private TerrainManager   m_TerrainManager;

    [SerializeField, Tooltip("Height above the terrain that the pathfinder area contains. Used to allow creature to go above the flight ceiling")]
    private float            m_PathableHeightAboveCeiling = 5f;

    [SerializeField, Tooltip("Minimum size nodes will subdivide to")]
    private float            m_MinimumNodeSize = 1f;

    [SerializeField, Tooltip("Physics layers to test for obstacles")]
    private LayerMask        m_ImpassableLayers = Physics.AllLayers;

    public static OctreePathfinder Instance { get; private set; }
    public bool              Initialised => m_PassableTree != null;
    public int               NodeCount => m_PassableTree.Nodes.Count;

    public int BreakIndex = -1;

    private void GeneratePassableTree()
    {
        Profiler.BeginSample("Pathinder - Generate Tree");

        float bounds_height = (Mathf.Max(0f, m_PathableHeightAboveCeiling) + m_TerrainManager.Definition.MaxHeight);
        Bounds tree_bounds = new Bounds(transform.position + Vector3.up * bounds_height * 0.5f, m_TerrainManager.PlayableTerrainArea.size.XZ() + Vector3.up * bounds_height);
        m_PassableTree = new Octree<bool>(tree_bounds, true);

        List<OctreeNode<bool>> open_nodes = new List<OctreeNode<bool>>(256);

        open_nodes.Add(m_PassableTree.Root);

        while (!open_nodes.Empty())
        {
            OctreeNode<bool> current_node = open_nodes.PopBack();
            int node_index = m_PassableTree.FindNodeIndex(current_node);

            if (node_index == BreakIndex)
            {
                Debug.DebugBreak();
            }

            Vector3 current_node_extents = current_node.Bounds.extents;
            if (Physics.CheckBox(current_node.Bounds.center, current_node_extents, Quaternion.identity, m_ImpassableLayers))
            {
                //Split the node if still large enough
                float smallest_child_axis = Mathf.Min(current_node_extents.x, current_node_extents.y, current_node_extents.z) * 0.5f;
                if (m_MinimumNodeSize <= smallest_child_axis)
                {
                    m_PassableTree.SplitNodeAndAppend(current_node, open_nodes);
                }
                //Flag as an impassable leaf
                else
                {
                    m_PassableTree.SetNodeData(current_node, false);
                }
            }
        }

        Profiler.EndSample();
    }

    private IEnumerator CoInitialise()
    {
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
