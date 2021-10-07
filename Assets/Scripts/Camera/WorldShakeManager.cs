using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldShakeManager : MonoBehaviour
{
    public static WorldShakeManager Instance = null;
    private static List<ShakeableObject> m_Shakeables = new List<ShakeableObject>();

    [SerializeField] private float m_ShakeDuration = 0f;
    [SerializeField] private float m_ShakeMagnitude = 0.2f;

    //Unity Events
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError($"Error: Multiple shake managers exist. Objects: {Instance.name} | {name}");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            m_Shakeables.Clear();
        }
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1))
        {
            m_ShakeDuration = 3f;
            m_ShakeMagnitude = .2f;
        }
#endif

        if (m_ShakeDuration > 0f)
        {
            m_ShakeDuration -= Time.deltaTime;

            //Double check so we can stop shaking. Don't attempt to stop shaking on every object every frame
            if (m_ShakeDuration > 0f)
            {
                foreach (ShakeableObject obj in m_Shakeables)
                {
                    obj.Shake(m_ShakeMagnitude);
                }
            }
            else
            {
                StopShake();
            }
        }
    }

    //Methods
    public void Shake(float magnitude, float duration)
    {
        if (magnitude >= m_ShakeMagnitude)
        {
            m_ShakeDuration = duration;
            m_ShakeMagnitude = magnitude;
        }
    }

    public void StopShake()
    {
        m_ShakeDuration = m_ShakeMagnitude = 0f;

        foreach (ShakeableObject obj in m_Shakeables)
        {
            obj.StopShake();
        }
    }

    public static void AddShakeable(ShakeableObject obj)
    {
        m_Shakeables.Add(obj);
    }

    public static void RemoveShakeable(ShakeableObject obj)
    {
        m_Shakeables.Remove(obj);
    }
}
