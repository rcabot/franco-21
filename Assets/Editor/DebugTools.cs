using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

class DebugTools : EditorWindow
{
    [SerializeField]
    bool m_ShowShakeTest = false;
    float m_ShakeTestMagnitude = 0f;
    float m_ShakeTestDuration = 0f;

    [SerializeField]
    bool m_ShowCreatureTest = false;
    Vector3 m_HunterTeleportPosition = Vector3.zero;

    [SerializeField]
    bool m_ShowTerrainTest = false;
    bool m_DrawTerrainBounds = false;

    [SerializeField]
    bool m_ShowOctreePathfidingTest = false;

    [SerializeField]
    bool m_ShowPickupTest = false;
    [SerializeField]
    int m_PickupDestroyAmount = 1;

    [MenuItem("Franco Jam/Debug Tools")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<DebugTools>();
    }

    private void Update()
    {
        if (m_DrawTerrainBounds)
        {
            TerrainManager terrain = TerrainManager.Instance;

            if (terrain != null)
            {
                Rect bounds = TerrainManager.Instance.PlayableTerrainArea;

                Debug.DrawLine(bounds.min.XZ(), bounds.min.XZ() + Vector3.up * 100f, Color.magenta, 1f, false);
                Debug.DrawLine(bounds.max.XZ(), bounds.max.XZ() + Vector3.up * 100f, Color.cyan, 1f, false);
            }
        }
    }

    void OnGUI()
    {
        if (Application.isPlaying)
        {
            CameraShakeUI();
            CreatureUI();
            TerrainUI();
            OctreePathfindingUI();
            PickupTest();
        }
        else
        {
            EditorGUILayout.LabelField("Waiting for game to start...");
        }
    }

    private void CreatureUI()
    {
        m_ShowCreatureTest = EditorGUILayout.Foldout(m_ShowCreatureTest, "Creature Test");
        if (m_ShowCreatureTest)
        {
            HunterBehaviour hunter = HunterBehaviour.Instance;
            if (hunter == null)
            {
                EditorGUILayout.LabelField("No Creature Found...");
                return;
            }

            HunterBehaviour.EnableLogging = EditorGUILayout.Toggle("Enable Logging", HunterBehaviour.EnableLogging);
                
            HunterState new_state = (HunterState)EditorGUILayout.EnumPopup("Current State", hunter.CurrentState);
            if (new_state != hunter.CurrentState)
            {
                hunter.ForceSetState(new_state);
            }

            hunter.PlayerAggro = EditorGUILayout.Slider("Player Aggro", hunter.PlayerAggro, 0, 100);

            if (GUILayout.Button("Force Backstage"))
            {
                hunter.ForceSetState(HunterState.Backstage);
            }

            m_HunterTeleportPosition = EditorGUILayout.Vector3Field("Teleport Position", m_HunterTeleportPosition);
            if (GUILayout.Button("Teleport"))
            {
                hunter.DebugTeleport(m_HunterTeleportPosition);
            }
        }
    }

    private void CameraShakeUI()
    {
        m_ShowShakeTest = EditorGUILayout.Foldout(m_ShowShakeTest, "Shake Test");
        if (m_ShowShakeTest)
        {
            if (WorldShakeManager.Instance == null)
            {
                EditorGUILayout.LabelField("No shake manager found...");
                return;
            }

            m_ShakeTestMagnitude = EditorGUILayout.Slider("Magnitude", m_ShakeTestMagnitude, 0f, 10f);
            m_ShakeTestDuration = EditorGUILayout.Slider("Duration (s)", m_ShakeTestDuration, 0f, 10f);
            if (GUILayout.Button("Test Shake"))
            {
                WorldShakeManager.Shake(m_ShakeTestMagnitude, m_ShakeTestDuration);
            }
            if (GUILayout.Button("Stop"))
            {
                WorldShakeManager.StopShake();
            }
        }
    }

    private void TerrainUI()
    {
        m_ShowTerrainTest = EditorGUILayout.Foldout(m_ShowTerrainTest, "Terrain"); ;
        if (m_ShowTerrainTest)
        {
            if (TerrainManager.Instance == null)
            {
                EditorGUILayout.LabelField("No terrain manager found...");
                return;
            }

            m_DrawTerrainBounds = EditorGUILayout.Toggle("Draw Terrain Bounds", m_DrawTerrainBounds);

            if (GUILayout.Button("Force Refresh Terrain Heights"))
            {
                foreach (TerrainTile t in FindObjectsOfType<TerrainTile>())
                {
                    t.ForceRefresh();
                }
            }
        }
    }

    private void OctreePathfindingUI()
    {
        m_ShowOctreePathfidingTest = EditorGUILayout.Foldout(m_ShowOctreePathfidingTest, "Octree Pathfinding");
        if (m_ShowOctreePathfidingTest)
        {
            if (OctreePathfinder.Instance == null)
            {
                EditorGUILayout.LabelField("No pathfinder found...");
                return;
            }
            OctreePathfinder.Instance.Debug_ActiveNodeDrawFlags = (OctreePathfinder.DebugNodeDrawFlags)EditorGUILayout.EnumFlagsField("Debug Node Draw", OctreePathfinder.Instance.Debug_ActiveNodeDrawFlags);

            OctreePathfinder.Instance.Debug_HighlightNodeIndex = EditorGUILayout.IntSlider("Highlight Node", OctreePathfinder.Instance.Debug_HighlightNodeIndex, -1, OctreePathfinder.Instance.NodeCount - 1);

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Total Nodes", OctreePathfinder.Instance.NodeCount);
            EditorGUILayout.BoundsField("Highlighted Bounds", OctreePathfinder.Instance.Debug_HighlightNode.Bounds);
            EditorGUILayout.LabelField("Highlighted Octant", OctreePathfinder.Instance.Debug_HighlightNode.Octant.ToString());
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Highlight Adjacent", EditorStyles.miniButton))
            {
                OctreePathfinder.Instance.DebugHighlightAdjacentNodes();
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Tree", EditorStyles.miniButtonLeft))
            {
                OctreePathfinder.Instance.DebugClearTree();
            }

            if (GUILayout.Button("Split Nodes", EditorStyles.miniButtonMid))
            {
                OctreePathfinder.Instance.DebugSplitTreeNodes();
            }

            if (GUILayout.Button("Regenerate Tree", EditorStyles.miniButtonRight))
            {
                OctreePathfinder.Instance.DebugRegeneratePassableTree();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void PickupTest()
    {
        m_ShowPickupTest = EditorGUILayout.Foldout(m_ShowPickupTest, "Pickup Test");
        if (m_ShowPickupTest)
        {
            if (PlayerState.Instance == null)
            {
                EditorGUILayout.LabelField("No player state active...");
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.IntField("Total", PlayerState.Instance.TotalCollectables);
            EditorGUILayout.IntField("Remaining", PlayerState.Instance.CalculateLeftToCollect);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_PickupDestroyAmount = EditorGUILayout.IntSlider(m_PickupDestroyAmount, 1, PlayerState.Instance?.CalculateLeftToCollect ?? 1);
            if (GUILayout.Button("Destroy Pickup", EditorStyles.miniButtonRight))
            {
                foreach (Collectable o in FindObjectsOfType<Collectable>().Take(m_PickupDestroyAmount))
                {
                    Destroy(o.gameObject);
                }
                PlayerState.Instance?.CollecedItem();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Destroy All Pickups"))
            {
                foreach (Collectable o in FindObjectsOfType<Collectable>())
                {
                    Destroy(o.gameObject);
                }

                PlayerState.Instance?.CollecedItem();
            }
        }
    }
}
