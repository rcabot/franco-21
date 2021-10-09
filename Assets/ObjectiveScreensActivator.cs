using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ObjectiveScreensActivator : MonoBehaviour
{
    [SerializeField] private InputActionReference PressStartAction;
    [SerializeField] private InputActionReference QuitAction;
    public GameObject VictoryScreen;
    public GameObject DefeatScreen;


    private void Start()
    {
        PlayerState.Instance.OnGameStateChanged += OnGameStateChanged;
        QuitAction.action.performed += OnQuitPressed;
    }

    private void OnDestroy()
    {
        PressStartAction.action.performed -= OnStartPressed;
        QuitAction.action.performed -= OnQuitPressed;
    }

    private void OnGameStateChanged(PlayerState.State prev, PlayerState.State next)
    {
        if (prev == next)
            return;

        switch (next)
        {
            case PlayerState.State.Victory:
            case PlayerState.State.Defeat:
                PressStartAction.action.performed += OnStartPressed;
                VictoryScreen.SetActive(PlayerState.Instance.GameState == PlayerState.State.Victory);
                DefeatScreen.SetActive(PlayerState.Instance.GameState == PlayerState.State.Defeat);
                break;
        }
    }

    private void OnStartPressed(InputAction.CallbackContext obj)
    {
        //Return to the frontend
        SceneManager.LoadScene(0);
    }

    private void OnQuitPressed(InputAction.CallbackContext obj)
    {
#if !UNITY_EDITOR
        //Return to the frontend
        SceneManager.LoadScene(0);
#endif
    }
}
