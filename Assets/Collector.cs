using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : MonoBehaviour
{
    private PlayerState m_playerState;
    AudioSource trashCollect;

    private void Start()
    {
        trashCollect = GetComponent<AudioSource>();
    }

    void Awake()
    {
        m_playerState = FindObjectOfType<PlayerState>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Collectable>())
        {
            Destroy(other.gameObject);
            trashCollect.Play();
        }

    }
}
