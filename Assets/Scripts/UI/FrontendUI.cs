using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FrontendUI : MonoBehaviour
{
    private ContinueText m_ContinueText;
    public InputActionReference QuitAction;
    public InputActionReference NextSceneAction;
    public int NextScene = 1;

    private void Start()
    {
        m_ContinueText = FindObjectOfType<ContinueText>();
        NextSceneAction.action.performed += OnNextScenePressed;
        FullscreenFade.OnFadeCompleted += OnFadeCompleted;

        if (QuitAction)
        {
            QuitAction.action.performed += OnQuit;
        }
    }

    private void OnDestroy()
    {
        NextSceneAction.action.performed -= OnNextScenePressed;

        if (QuitAction)
        {
            QuitAction.action.performed -= OnQuit;
        }
    }

    private void OnQuit(InputAction.CallbackContext obj)
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }

    private void OnNextScenePressed(InputAction.CallbackContext obj)
    {
        FullscreenFade.FadeOut();
    }

    private void OnFadeCompleted(FadeDirection fade_dir)
    {
        if (fade_dir == FadeDirection.Out)
        {
            m_ContinueText?.SetText("Loading...");
            SceneManager.LoadSceneAsync(NextScene);
            NextSceneAction.action.performed -= OnNextScenePressed;
        }
    }
}
