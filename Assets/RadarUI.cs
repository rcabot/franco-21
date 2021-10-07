using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;

public class RadarUI : MonoBehaviour
{
    public GameObject[] CollectablesBlips;
    public GameObject PortalBlip;
    public Transform CentreWaypoint;
    public Transform RotationWaypoint;
    public RectTransform Background;
    private CollectablesDistributor CollectablesRegistry;
    private VictoryPortal VictoryPortal;

    public Image Altimeter = null;

    public float RadarDistance = 100f;

    private void Awake()
    {
        CollectablesRegistry = FindObjectOfType<CollectablesDistributor>();
        VictoryPortal = FindObjectOfType<VictoryPortal>();
    }

    void FixedUpdate()
    {
        var collectables = CollectablesRegistry.SpawnedCollectables.Where(s => s != null).OrderBy(s => Vector3.Distance(s.transform.position, CentreWaypoint.position)).ToArray();
        for (int i = 0; i < CollectablesBlips.Length; i++)
        {
            bool active = i < collectables.Length;
            CollectablesBlips[i].SetActive(active);
            if (active)
            {
                PositionBlip(CollectablesBlips[i], collectables[i]);
            }
        }

        bool portalActive = PlayerState.Instance.GameState == PlayerState.State.ObjectiveComplete;
        PortalBlip.SetActive(portalActive);
        if (portalActive)
        {
            PositionBlip(PortalBlip, VictoryPortal.gameObject);
        }
        var radarRotation = Background.eulerAngles;
        radarRotation.z = RotationWaypoint.eulerAngles.y;
        Background.eulerAngles = radarRotation;

        // Altimeter
        float normAltitude = Mathf.Clamp(transform.position.y / TerrainManager.Instance.Definition.MaxHeight, 0.0f, 1.0f);
        Altimeter.fillAmount = normAltitude;
    }

    private void PositionBlip(GameObject blip, GameObject inWorldObject)
    {
        var radarRadius = Background.rect.width / 2f;
        var displacement = inWorldObject.transform.position - CentreWaypoint.position;
        var direction = displacement.normalized;
        var distanceFromRadarCentre = Mathf.InverseLerp(0, RadarDistance, displacement.magnitude);
        ((RectTransform)blip.transform).anchoredPosition = radarRadius * distanceFromRadarCentre * new Vector2(
            direction.x,
            direction.z);
        
    }
}
