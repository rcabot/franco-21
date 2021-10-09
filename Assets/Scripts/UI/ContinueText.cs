using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ContinueText : MonoBehaviour
{
    [SerializeField] float m_EllipsesTimeScale = 0.2f;
    float                  m_EllipsesProgress = 0f;
    Text                   m_Text;
    string                 m_BaseContent;
    StringBuilder          m_StringBuilder = new StringBuilder();

    private void Awake()
    {
        m_Text = this.RequireComponent<Text>();
        m_BaseContent = m_Text.text;
    }

    private void Update()
    {
        int prev_dots_to_add = Mathf.FloorToInt(m_EllipsesProgress);
        m_EllipsesProgress = (m_EllipsesProgress + Time.deltaTime * m_EllipsesTimeScale) % 4;
        int new_dots_to_add = Mathf.FloorToInt(m_EllipsesProgress);

        if (prev_dots_to_add != new_dots_to_add)
        {
            m_StringBuilder.Clear();
            m_StringBuilder.Append(m_BaseContent);
            for (int i = 0; i < new_dots_to_add; ++i)
            {
                m_StringBuilder.Append('.');
            }
            m_Text.text = m_StringBuilder.ToString();
        }
    }
}
