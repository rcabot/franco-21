using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveScreensActivator : MonoBehaviour
{
    public GameObject VictoryScreen;
    public GameObject DefeatScreen;
    // Update is called once per frame
    void Update()
    {
        VictoryScreen.SetActive(PlayerState.Instance.GameState == PlayerState.State.Victory);
        DefeatScreen.SetActive(PlayerState.Instance.GameState == PlayerState.State.Defeat);
    }
}
