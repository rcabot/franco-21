using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class intro_collision_event_handler : MonoBehaviour
{
    public event Action<Collider> EventCollidedWithGround;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        EventCollidedWithGround?.Invoke(other);
    }
}
