using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class FrontendUI : MonoBehaviour
{
    public InputActionReference NextSceneAction;
    public int NextScene = 1;

    private void Start()
    {
        NextSceneAction.action.performed += OnNextScenePressed;
    }

    private void OnNextScenePressed(InputAction.CallbackContext obj)
    {
        SceneManager.LoadScene(NextScene);
        NextSceneAction.action.performed -= OnNextScenePressed;
    }
}
