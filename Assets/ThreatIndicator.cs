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
            Color indicatorColor = new Color(0, 0, 0);
            if (normAlertLevel < 0.3f)
            {
                indicatorColor.g = 1.0f;
            }
            else if (normAlertLevel < 0.6f)
            {
                indicatorColor.g = 1.0f;
                indicatorColor.r = 1.0f;
            }
            else
            {
                indicatorColor.r = 1.0f;
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
