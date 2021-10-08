using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ObjectiveScreensActivator : MonoBehaviour
{
    [SerializeField] private InputActionReference PressStartAction;
    public GameObject VictoryScreen;
    public GameObject DefeatScreen;


    private void Start()
    {
        PlayerState.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        PressStartAction.action.performed -= OnStartPressed;
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
}
