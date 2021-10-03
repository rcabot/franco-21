﻿using System;
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
    Vector3 m_HunterTeleportPosition = Vector3.zero;

    [MenuItem("Franco Jam/Debug Tools")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<DebugTools>();
    }

    void OnGUI()
    {
        if (Application.isPlaying)
        {
            CameraShakeUI();
            CreatureUI();
        }
        else
        {
            EditorGUILayout.LabelField("Waiting for game to start...");
        }
    }

    private void CreatureUI()
    {
        HunterBehaviour hunter = HunterBehaviour.Instance;
        if (hunter != null)
        {
            m_ShowCreatureTest = EditorGUILayout.Foldout(m_ShowCreatureTest, "Creature Test");
            if (m_ShowCreatureTest)
            {
                HunterBehaviour.EnableLogging = EditorGUILayout.Toggle("Enable Logging", HunterBehaviour.EnableLogging);
                
                HunterState new_state = (HunterState)EditorGUILayout.EnumPopup("Current State:", hunter.CurrentState);
                if (new_state != hunter.CurrentState)
                {
                    hunter.ForceSetState(new_state);
                }

                hunter.PlayerAggro = EditorGUILayout.IntSlider("Player Aggro", hunter.PlayerAggro, 0, 100);

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
        else
        {
            EditorGUILayout.LabelField("- No Creature Found -");
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
