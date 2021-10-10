using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlsOverlay : MonoBehaviour
{
    public bool Active;
    public GameObject Overlay;
    [SerializeField] private InputActionReference viewControlsAction;
    // Start is called before the first frame update
    void Start()
    {
        Overlay.SetActive(false);
        viewControlsAction.action.performed += cb =>
        {
            Active = !Active;
        };
    }

    // Update is called once per frame
    void Update()
    {
        Overlay.SetActive(Active);
    }
}
