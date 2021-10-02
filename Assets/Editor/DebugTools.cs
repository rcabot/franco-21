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

    [MenuItem("Franco Jam/Debug Tools")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow<DebugTools>();
    }

    public void OnGUI()
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
