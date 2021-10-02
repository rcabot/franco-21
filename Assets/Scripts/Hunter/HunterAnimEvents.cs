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
    private Collider m_BiteCollider;

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_BiteCollider = GetComponentInChildren<SphereCollider>();
    }

    public void EnableBiteCollider()
    {
        m_BiteCollider.enabled = true;
    }

    public void DisableBiteCollider()
    {
        m_BiteCollider.enabled = false;
    }
}
