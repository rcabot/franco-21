using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFacingSprite : MonoBehaviour
{
    private Camera m_camera;

    void Start()
    {
        m_camera = Camera.main;
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(0f, m_camera.transform.rotation.eulerAngles.y, 0f);
    }
}
