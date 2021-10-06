using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(Animator))]
class HunterAnimEvents : MonoBehaviour  
{
    private Animator m_Animator;
    public AudioSource biteAudioSource = null;
    public AudioClip biteAudio = null;
    public AudioClip preBiteAudio = null;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    public void TriggerBiteAudio()
    {
        biteAudioSource.clip = biteAudio;
        biteAudioSource.Play();
    }

    public void TriggerPreBiteAudio()
    {
        biteAudioSource.clip = preBiteAudio;
        biteAudioSource.Play();
    }

    public void EnableBiteCollider()
    {
        if (HunterBehaviour.Instance != null)
            HunterBehaviour.Instance.AttackEnabled = true;
    }

    public void DisableBiteCollider()
    {
        if (HunterBehaviour.Instance != null)
            HunterBehaviour.Instance.AttackEnabled = false;
    }
}
