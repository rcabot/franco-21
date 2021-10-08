using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FrontendUI : MonoBehaviour
{
    public int NextScene = 1;

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(NextScene);
            enabled = false;
        }
    }
}
