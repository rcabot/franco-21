using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class WorldShakeManager : MonoBehaviour
{
    public static WorldShakeManager Instance = null;
    private static List<ShakeableObject> m_Shakeables = new List<ShakeableObject>();

    private bool m_VibrationActive = false;
    private bool VibrationActive
    {
        get => m_VibrationActive;
        set
        {
            if (m_VibrationActive != value)
            {
                m_VibrationActive = value;

                if (!value)
                {
                    Gamepad.current?.SetMotorSpeeds(0f, 0f);
                }
                else if (m_ShakeDuration > 0.1f)
                {
                    Gamepad.current?.SetMotorSpeeds(1f, 1f);
                }
            }
        }
    }

    private float m_ShakeDuration = 0f;
    private float m_ShakeMagnitude = 0f;

    //Unity Events
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            foreach (PlayerInput pi in PlayerInput.all)
            {
                pi.onActionTriggered += onInputActionTriggered;
            }
        }
        else
        {
            Debug.LogError($"Error: Multiple shake managers exist. Objects: {Instance.name} | {name}");
            Destroy(this);
        }
    }

    private void onInputActionTriggered(InputAction.CallbackContext obj)
    {
        Gamepad pad = obj.control.device as Gamepad;
        VibrationActive = pad != null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            m_Shakeables.Clear();

            foreach (PlayerInput pi in PlayerInput.all)
            {
                pi.onActionTriggered -= onInputActionTriggered;
            }
        }

        Gamepad.current?.SetMotorSpeeds(0f, 0f);
    }

    private void LateUpdate()
    {
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
            if (VibrationActive)
            {
                Gamepad.current?.SetMotorSpeeds(1.0f, 1.0f);
            }

            m_ShakeDuration = duration;
            m_ShakeMagnitude = magnitude;
        }
    }

    public void StopShake()
    {
        m_ShakeDuration = m_ShakeMagnitude = 0f;

        Gamepad.current?.SetMotorSpeeds(0f, 0f);
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
