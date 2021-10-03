using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class SubmarineController : MonoBehaviour
{
    private LightActivator lights;
    private TractorBeamActivator tractor_beam;
    float acceleration = 7.5f;
    public Camera playerCamera;
    public Transform submarineCockpit;
    public float baseLookSpeed = 2.0f;
    public float maxLookSpeed = 2.0f;
    public float lookXLimit = 45.0f;
    public float strafeFactor = 0.0f;
    public float maxAudioSpeed = 10.0f;

    public AnimationCurve frictionCurve;

    public float lookSmoothTime;

    public bool enableGradualRotation = false;

    Rigidbody rigidBody;
    Vector2 targetLookRotation;
    Vector2 currentLookRotation;
    public Vector3 currentSpeed = Vector3.zero;
    Vector2 currentLookVelocity;
    AudioSource engineSound;
    AudioSource underwaterSound;
    AudioSource scrapingSound;


    AudioSource shipAudioSource;
    public AudioClip smallBonk;
    public AudioClip bigBonk;


    public float collisionElasticity = 0.5f;
    public float collisionHardThreshold = 12.0f;
    private bool hasCollidedThisFrame = false;
    private bool scraping = false;

    public enum MovementGear
    {
        SLOW,
        NORMAL,
        FAST
    }
    public MovementGear currentGear = MovementGear.SLOW;
    int num_gears = Enum.GetNames(typeof(MovementGear)).Length;
    public float[] gearSpeeds = { 2, 5, 10 };

    public bool LightsOn => lights?.LightsEnabled ?? false;

    public bool TractorBeamOn => tractor_beam?.TractorActive ?? false;

    private void Awake()
    {
        lights = GetComponent<LightActivator>();
        tractor_beam = GetComponentInChildren<TractorBeamActivator>();
    }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        engineSound = transform.Find("ship_engine").GetComponent<AudioSource>();
        underwaterSound = transform.Find("water_ambience").GetComponent<AudioSource>();
        scrapingSound = transform.Find("ship_scrape").GetComponent<AudioSource>();
        shipAudioSource = GetComponent<AudioSource>();
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentGear = MovementGear.SLOW;
        acceleration = gearSpeeds[(int)currentGear];
    }

    void Update()
    {
        if( scraping)
        {
            if (scrapingSound.isPlaying == false)
            {
                scrapingSound.Play();
            }
            scrapingSound.volume = currentSpeed.magnitude / maxAudioSpeed;
        }
        else if( scraping == false && scrapingSound.isPlaying)
        {
            scrapingSound.Stop();
        }

        UpdateLookDirection();
        if( Input.GetButtonDown("toggle_gear"))
        {
            ToggleCurrentGear();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TakeHit();
        }
    }

    private void FixedUpdate()
    {
        UpdateMovement();
        hasCollidedThisFrame = false;
    }


    void ToggleCurrentGear()
    {
        currentGear = (MovementGear)(((int)currentGear + 1) % num_gears);
        acceleration = gearSpeeds[(int)currentGear];
    }

    private void UpdateMovement()
    {
        Vector3 forward = submarineCockpit.transform.forward;
        Vector3 right = submarineCockpit.transform.right;

        Vector3 strafe_acceleration = right * Input.GetAxis("Horizontal") * strafeFactor;
        Vector3 forward_acceleration = forward * Input.GetAxis("Vertical");

        Vector3 input_acceleration = ((strafe_acceleration + forward_acceleration).normalized * acceleration) * Time.fixedDeltaTime;

        // acceleration
        currentSpeed += input_acceleration;

        // modify friction with speed
        float current_speed_magnitude = currentSpeed.magnitude;
        Vector3 current_speed_direction = currentSpeed / current_speed_magnitude;
        float friction = frictionCurve.Evaluate(current_speed_magnitude) * Time.deltaTime;

        // apply friction
        if (current_speed_magnitude > 0) 
            currentSpeed = current_speed_direction * Mathf.Max(0f, current_speed_magnitude - friction);

        // clamp speed

        engineSound.pitch = 1 + (4 * (current_speed_magnitude / maxAudioSpeed));
        underwaterSound.pitch = 1 + (1 * (current_speed_magnitude / maxAudioSpeed));
        // todo: buoyancy
        /*if (Input.GetButton("Jump"))
        {
        }
        if (Input.GetButton("Crouch"))
        {
        }*/

        // Move the controller
        rigidBody.MovePosition(rigidBody.position + currentSpeed * Time.fixedDeltaTime);
        //characterController.Move(currentSpeed * Time.deltaTime*speedScale);
    }

    private void UpdateLookDirection()
    {
        // Player and Camera rotation
        targetLookRotation.x += -Input.GetAxis("Mouse Y") * baseLookSpeed;
        targetLookRotation.x = Mathf.Clamp(targetLookRotation.x, -lookXLimit, lookXLimit);
        targetLookRotation.y += Input.GetAxis("Mouse X") * baseLookSpeed;
        var prevLook = targetLookRotation - currentLookRotation;
        currentLookRotation = Vector2.SmoothDamp(currentLookRotation, targetLookRotation, ref currentLookVelocity, lookSmoothTime,maxLookSpeed);
        if (enableGradualRotation)
        {
            targetLookRotation = currentLookRotation + prevLook;
        }
        playerCamera.transform.rotation = Quaternion.Euler(targetLookRotation.x, targetLookRotation.y, 0);
        submarineCockpit.localRotation = Quaternion.Euler(currentLookRotation.x, currentLookRotation.y, 0);
        //transform.rotation *= Quaternion.Euler(0, currentLookSpeed.y, 0);
    }

    public void AddImpulse(Vector3 force)
    {
        currentSpeed += force;
    }

    public void TakeHit()
    {
        WorldShakeManager.Instance.Shake(1.0f, 1.0f);
        PlayerState.Instance.Health--;
        Debug.Log("Player Hit");
    }

    private void OnCollisionEnter(Collision collision)
    {
        scraping = true;
        if (hasCollidedThisFrame == false)
        {
            hasCollidedThisFrame = true;
            float collision_intensity = Mathf.Abs(Vector3.Dot(collision.contacts[0].normal, currentSpeed) * currentSpeed.magnitude);
            if (collision_intensity > collisionHardThreshold)
            {
                shipAudioSource.clip = bigBonk;
            }
            else
            {
                shipAudioSource.clip = smallBonk;
            }

            AddImpulse((collision_intensity * collisionElasticity) * collision.contacts[0].normal);

            shipAudioSource.Play();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        scraping = false;
    }
}
