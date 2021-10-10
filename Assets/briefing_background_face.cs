using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class briefing_background_face : MonoBehaviour
{

    private Camera m_camera;
    private Color m_startColor;
    public Color m_targetColor;

    public float m_delayUntilTransitionTime = 90.0f;
    public float m_transitionTime = 30.0f;
    private float m_timeAccrued = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_camera = GetComponent<Camera>();
        m_startColor = m_camera.backgroundColor;
        m_timeAccrued = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        m_timeAccrued += Time.deltaTime;

        float fadeProgress = Mathf.Clamp(m_timeAccrued - m_delayUntilTransitionTime, 0.0f, m_transitionTime) / m_transitionTime;

        m_camera.backgroundColor = m_startColor + ((m_targetColor - m_startColor) * fadeProgress);
    }
}
