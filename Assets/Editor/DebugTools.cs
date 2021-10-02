using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

class DebugTools : EditorWindow
{
    bool m_ShowShakeTest = false;
    float m_ShakeTestMagnitude = 0f;
    float m_ShakeTestDuration = 0f;

    bool m_ShowCreatureTest = false;
    HunterState m_OverrideState = null;
    HunterBehaviour m_HunterBehaviour = null;

    [MenuItem("Franco Jam/Debug Tools")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<DebugTools>();
    }

    private void OnEnable()
    {
        m_HunterBehaviour = FindObjectOfType<HunterBehaviour>();
    }

    void OnGUI()
    {
        CameraShakeUI();
        CreatureUI();
    }

    private void CreatureUI()
    {
        if (m_HunterBehaviour)
        {
            m_ShowCreatureTest = EditorGUILayout.Foldout(m_ShowCreatureTest, "Creature Test");
            if (m_ShowCreatureTest)
            {
                m_HunterBehaviour.PlayerAttention = EditorGUILayout.IntSlider("Player Aggro", m_HunterBehaviour.PlayerAttention, 0, 100);
                m_OverrideState = EditorGUILayout.ObjectField("Override State", m_OverrideState, typeof(HunterBehaviour), false) as HunterState;

                if (m_OverrideState != null && GUILayout.Button("Appply State Override"))
                {
                    m_HunterBehaviour.ForceSetState(m_OverrideState);
                }
            }
        }
    }

    private void CameraShakeUI()
    {
        m_ShowShakeTest = EditorGUILayout.Foldout(m_ShowShakeTest, "Shake Test");
        if (m_ShowShakeTest)
        {
            m_ShakeTestMagnitude = EditorGUILayout.Slider("Magnitude", m_ShakeTestMagnitude, 0f, 10f);
            m_ShakeTestDuration = EditorGUILayout.Slider("Duration (s)", m_ShakeTestDuration, 0f, 10f);
            if (GUILayout.Button("Test Shake"))
            {
                WorldShakeManager.Instance?.Shake(m_ShakeTestMagnitude, m_ShakeTestDuration);
            }
            if (GUILayout.Button("Stop"))
            {
                WorldShakeManager.Instance?.StopShake();
            }
        }
    }
}
