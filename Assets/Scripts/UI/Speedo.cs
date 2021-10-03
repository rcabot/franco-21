using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Speedo : MonoBehaviour
{
    [SerializeField] private Sprite[] SpeedoStates;
    private Image m_Image;
    private SubmarineController m_PlayerSub = null;

    private void Awake()
    {
        m_Image = GetComponent<UnityEngine.UI.Image>();
        m_PlayerSub = FindObjectOfType<SubmarineController>();
        m_PlayerSub.OnGearChanged += OnGearChanged;
    }

    private void OnDestroy()
    {
        m_PlayerSub.OnGearChanged -= OnGearChanged;
    }

    private void OnGearChanged(SubmarineController player_sub, SubmarineController.MovementGear new_gear)
    {
        if (SpeedoStates.Length == 0)
            return;

        m_Image.sprite = SpeedoStates[Mathf.Min(SpeedoStates.Length - 1, (int)player_sub.currentGear)];
    }
}
