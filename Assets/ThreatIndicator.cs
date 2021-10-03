using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreatIndicator : MonoBehaviour
{
    [SerializeField, Tooltip("Submarine transform")] private Transform SubTransform = null;
    [SerializeField, Tooltip("Range at which the indicator starts up")] private float RangeThreshold = 1.0f;
    [SerializeField, Tooltip("Number of squares to fill on the indicator")] private int IndicatorCount = 1;
    //[SerializeField, Tooltip("Sound made by the indicator when the creature is near")] private AudioSou _AlertSound = null;

    private int AlertLevel = 0;

    private void Awake()
    {
        // TODO: add a reference to the creature?
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        int prevAlertLevel = AlertLevel;

        Vector3 creaturePosition = HunterBehaviour.Instance.transform.position;
        float creatureDistance = Vector3.Distance(creaturePosition, SubTransform.position);
        if (creatureDistance < RangeThreshold)
        {
            //float stepSize = _RangeThreshold / _IndicatorCount
            // 
            AlertLevel = 1;
        }
        else
        {
            AlertLevel = 0;
        }

        if(prevAlertLevel != AlertLevel)
        {
            if (prevAlertLevel > 0)
            {
                Debug.Log("Escaped the creature!");
            }
            else
            {
                Debug.Log("Creature too close!");
            }
        }
    }
}
