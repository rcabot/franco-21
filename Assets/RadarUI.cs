using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class RadarUI : MonoBehaviour
{
    public GameObject[] CollectablesBlips;
    public GameObject[] WildlifeBlips;
    public Transform CentreWaypoint;
    public RectTransform Background;
    private WildlifeSpawner WildlifeRegistry;
    private CollectablesDistributor CollectablesRegistry;
    public float RadarDistance=100f;

    private void Awake()
    {
        WildlifeRegistry = FindObjectOfType<WildlifeSpawner>();
        CollectablesRegistry = FindObjectOfType<CollectablesDistributor>();
    }

    // Update is called once per frame
    void Update()
    {
        var wildlife = WildlifeRegistry.m_spawnedWildLife.Where(s=>s!=null).OrderBy(s => Vector3.Distance(s.transform.position, CentreWaypoint.position)).ToArray();
        for (int i = 0; i < WildlifeBlips.Length; i++)
        {
            bool active = i < wildlife.Length;
            WildlifeBlips[i].SetActive(active);
            if (active)
            {
                PositionBlip(WildlifeBlips[i], wildlife[i]);
            }
        }
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
