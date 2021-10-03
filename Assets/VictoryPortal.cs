using UnityEngine;

internal class VictoryPortal : MonoBehaviour
{
    public bool TouchedByPlayer { get; internal set; }
    public GameObject PortalCollider;

    private void Start()
    {
        
    }

    private void Update()
    {
        PortalCollider.SetActive(PlayerState.Instance.GameState == PlayerState.State.ObjectiveComplete);
    }
}