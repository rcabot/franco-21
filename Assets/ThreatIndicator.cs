using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThreatIndicator : MonoBehaviour
{
    //[SerializeField, Tooltip("Sound made by the indicator when the creature is near")] private AudioSou _AlertSound = null;
    [SerializeField] Sprite[] IndicatorTextures = null;

    Image[] m_Indicators;

    private int AlertLevel = -1;

    int GetIndicatorCellCount() { return IndicatorTextures.Length; }

    private void Awake()
    {
        m_Indicators = GetComponentsInChildren<Image>();
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateAlertLevel();
    }

    void UpdateAlertLevel()
    {
        int prevAlertLevel = AlertLevel;
        float normAlertLevel = HunterBehaviour.Instance.PlayerAggro / HunterBehaviour.Instance.AggroToAttack;
        int indicatorCellCount = GetIndicatorCellCount();

        AlertLevel = Mathf.FloorToInt(normAlertLevel * (indicatorCellCount - 1));
        if (prevAlertLevel != AlertLevel)
        {
            Color indicatorColor = Color.black;
            if (normAlertLevel >= 1.0f)
            {
                indicatorColor = Color.red;
            }
            else if (normAlertLevel >= 0.6f)
            {
                indicatorColor = Color.yellow;
            }
            else
            {
                indicatorColor = Color.green;
            }

            foreach (Image indicator in m_Indicators)
            {
                indicator.sprite = IndicatorTextures[AlertLevel];
                indicator.color = indicatorColor;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAlertLevel();
    }
}
