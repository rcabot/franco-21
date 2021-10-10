using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum FadeDirection
{
    In,
    Out,
    None
}

[RequireComponent(typeof(Image))]
public class FullscreenFade : MonoBehaviour
{
    private static FullscreenFade Instance { get; set; }
    public static event Action<FadeDirection> OnFadeCompleted;

    [SerializeField] float         m_FadeTime = 0.5f;
    [SerializeField] FadeDirection m_StartFade = FadeDirection.None;
    private Image          m_Image;
    private FadeDirection  m_CurrentFadeDirection = FadeDirection.None;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            m_Image = this.RequireComponent<Image>();
        }
        else
        {
            Debug.LogError("Error: Two fades present in scene");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        switch (m_StartFade)
        {
            case FadeDirection.In:
                FadeIn();
                break;
            case FadeDirection.Out:
                FadeOut();
                break;
            case FadeDirection.None:
            default:
                Color c = Instance.m_Image.color;
                c.a = 0f;
                Instance.m_Image.color = c;
                break;
        }
    }

    private void Update()
    {
        if (m_CurrentFadeDirection != FadeDirection.None)
        {
            float fade_speed = m_FadeTime > 0f ? 1.0f / m_FadeTime : 1f;
            Color current_colour = m_Image.color;
            float alpha = current_colour.a;
            bool fade_complete = false;

            switch (m_CurrentFadeDirection)
            {
                case FadeDirection.In:
                    alpha -= fade_speed * Time.deltaTime;
                    if (alpha <= 0f)
                    {
                        alpha = 0f;
                        fade_complete = true;
                    }
                    break;
                case FadeDirection.Out:
                    alpha += fade_speed * Time.deltaTime;
                    if (alpha >= 1f)
                    {
                        fade_complete = true;
                    }
                    break;
            }

            current_colour.a = Mathf.Clamp01(alpha);
            m_Image.color = current_colour;
            if (fade_complete)
            {
                FadeDirection completed_direction = m_CurrentFadeDirection;
                m_CurrentFadeDirection = FadeDirection.None;
                OnFadeCompleted?.Invoke(completed_direction);
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            OnFadeCompleted = null;
            Instance = null;
        }
    }

    public static void FadeOut()
    {
        if (Instance)
        {
            if (Instance.m_CurrentFadeDirection != FadeDirection.Out)
            {
                Instance.m_CurrentFadeDirection = FadeDirection.Out;
                Color c = Instance.m_Image.color;
                c.a = 0f;
                Instance.m_Image.color = c;
            }
        }
        else
        {
            OnFadeCompleted?.Invoke(FadeDirection.Out);
        }
    }

    public static void FadeIn()
    {
        if (Instance)
        {
            if (Instance.m_CurrentFadeDirection != FadeDirection.In)
            {
                Instance.m_CurrentFadeDirection = FadeDirection.In;
                Color c = Instance.m_Image.color;
                c.a = 1f;
                Instance.m_Image.color = c;
            }
        }
        else
        {
            OnFadeCompleted?.Invoke(FadeDirection.In);
        }
    }
}
