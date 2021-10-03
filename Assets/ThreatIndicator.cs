using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThreatIndicator : MonoBehaviour
{
    [SerializeField, Tooltip("Submarine transform")] private Transform SubTransform = null;
    [SerializeField, Tooltip("Range at which the indicator starts up")] private float RangeThreshold = 1.0f;
    //[SerializeField, Tooltip("Sound made by the indicator when the creature is near")] private AudioSou _AlertSound = null;

    [SerializeField] GameObject[] Indicators = null;
    [SerializeField] Texture[] IndicatorTextures = null;

    private int AlertLevel = 0;

    // Start is called before the first frame update
    void Start()
    {
        UpdateAlertLevel();
    }

    void UpdateAlertLevel()
    {
        foreach(GameObject currentIndicator in Indicators)
        {
            RawImage rawImage = currentIndicator.GetComponent<RawImage>();
            rawImage.texture = IndicatorTextures[AlertLevel];
        }
    }

    // Update is called once per frame
    void Update()
    {
        int prevAlertLevel = AlertLevel;

        Vector3 creaturePosition = HunterBehaviour.Instance.transform.position;
        float creatureDistance = Vector3.Distance(creaturePosition, SubTransform.position);
        if (creatureDistance < RangeThreshold)
        {
            float stepSize = RangeThreshold / IndicatorTextures.Length;
            AlertLevel = IndicatorTextures.Length - Mathf.FloorToInt(creatureDistance / stepSize);
        }
        else
        {
            AlertLevel = 0;
        }

        if(prevAlertLevel != AlertLevel)
        {
            UpdateAlertLevel();
        }
    }
}
