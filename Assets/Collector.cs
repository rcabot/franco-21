using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : MonoBehaviour
{
    private PlayerState m_playerState;

    void Awake()
    {
        m_playerState = FindObjectOfType<PlayerState>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectable"))
        {
            Destroy(other.gameObject);
            HunterBehaviour.Instance?.OnPlayerPickup();
            ++m_playerState.Collected;
        }

    }
}
