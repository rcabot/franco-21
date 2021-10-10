using UnityEngine;

internal class VictoryPortal : MonoBehaviour
{
    public bool TouchedByPlayer { get; internal set; }
    public MeshRenderer Portal = null;
    public Collider Collider = null;
    public float HeightAboveTerrain = 5.0f;

    private void Start()
    {
        transform.position = CollectablesDistributor.PlaceOnTerrain(transform.position) + HeightAboveTerrain * Vector3.up;
    }

    private void Update()
    {
        bool objectiveComplete = (PlayerState.Instance.GameState == PlayerState.State.ObjectiveComplete);
        Collider.enabled = objectiveComplete;
        Portal.gameObject.SetActive(objectiveComplete);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((PlayerState.Instance.GameState != PlayerState.State.ObjectiveComplete)) return;
        if (other.CompareTag("Player"))
        {
            TouchedByPlayer = true;
        }
    }
}