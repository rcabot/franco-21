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
        QuitAction.action.performed += OnQuit;
    }

    private void OnQuit(InputAction.CallbackContext obj)
    {
        Application.Quit();
    }

    private void OnNextScenePressed(InputAction.CallbackContext obj)
    {
        m_ContinueText?.SetText("Loading...");
        SceneManager.LoadSceneAsync(NextScene);
        NextSceneAction.action.performed -= OnNextScenePressed;
    }
}
