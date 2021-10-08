using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Animator), typeof(LightActivator))]

public class SubmarineController : MonoBehaviour
{
    private static readonly int c_CrackOverlayAmountID = Shader.PropertyToID("crack_amount");
    private static readonly int c_HitAnimTriggerID = Animator.StringToHash("Hit");
    [SerializeField] private float m_HitDisableTime = 3f;
    private Animator m_Animator;
    private LightActivator lights;
    private TractorBeamActivator tractor_beam;
    private Material crack_material;
    float acceleration = 7.5f;
    public Camera playerCamera;
    public Transform submarineCockpit;
    public Transform crackOverlay;
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
    AudioSource cabinAmbience;
    AudioSource underwaterSound;
    AudioSource scrapingSound;

    public AudioClip powerDownAudio;
    public AudioClip powerUpAudio;

    AudioSource shipAudioSource;
    public AudioClip smallBonk;
    public AudioClip bigBonk;
    public AudioClip damageBonk;


    public float collisionElasticity = 0.5f;
    public float collisionHardThreshold = 12.0f;
    private bool hasCollidedThisFrame = false;
    private bool scraping = false;

    public enum MovementGear
    {
        STOP,
        SLOW,
        NORMAL,
        FAST
    }
    public MovementGear currentGear = MovementGear.SLOW;
    int num_gears = Enum.GetNames(typeof(MovementGear)).Length;
    public float[] gearSpeeds = { 0, 2, 5, 10 };

    public bool LightsOn => lights?.LightsEnabled ?? false;

    private bool m_PowerOn = true;
    public bool PowerOn => m_PowerOn;

    public bool TractorBeamOn => tractor_beam?.TractorActive ?? false;

    public bool Scraping => scraping;

    public event Action<SubmarineController, MovementGear> OnGearChanged;
    public event Action OnTakeHit;

    private void Awake()
    {
        lights = this.RequireComponent<LightActivator>();
        lights.OnLightsToggled += OnLightsToggled;

        tractor_beam = GetComponentInChildren<TractorBeamActivator>();

        m_Animator = this.RequireComponent<Animator>();

        crack_material = crackOverlay?.RequireComponent<MeshRenderer>().material;
        crack_material.SetFloat(c_CrackOverlayAmountID, 0f);

        rigidBody = this.RequireComponent<Rigidbody>();
        engineSound = transform.Find("ship_engine").GetComponent<AudioSource>();
        underwaterSound = transform.Find("water_ambience").GetComponent<AudioSource>();
        scrapingSound = transform.Find("ship_scrape").GetComponent<AudioSource>();
        cabinAmbience = transform.Find("ship_ambience").GetComponent<AudioSource>();
        shipAudioSource = this.RequireComponent<AudioSource>();
    }

    void Start()
    {
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

        if( LightsOn && engineSound.isPlaying == false)
        {
            engineSound.Play();
            cabinAmbience.Play();
        }
        else if( LightsOn == false )
        {
            engineSound.Stop();
            cabinAmbience.Stop();
        }

        UpdateLookDirection();
        if( Input.GetButtonDown("toggle_gear") && LightsOn)
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
        SetCurrentGear( (MovementGear)(((int)currentGear + 1) % num_gears) );
    }

    void SetCurrentGear( MovementGear gear )
    {
        MovementGear old_gear = currentGear;
        currentGear = gear;
        acceleration = gearSpeeds[(int)currentGear];
        if (old_gear != currentGear)
        {
            OnGearChanged?.Invoke(this, currentGear);
        }
    }

    private void UpdateMovement()
    {
        Vector3 forward = submarineCockpit.transform.forward;
        Vector3 right = submarineCockpit.transform.right;

        Vector3 strafe_acceleration = LightsOn ? right * Mathf.Clamp(Input.GetAxis("Horizontal"), -1f, 1f) * strafeFactor : Vector3.zero;
        Vector3 forward_acceleration = LightsOn ? forward * Mathf.Clamp(Input.GetAxis("Vertical"), -1f, 1f) : Vector3.zero;

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
    }

    public void AddImpulse(Vector3 force)
    {
        currentSpeed += force;
    }

    public void TakeHit()
    {
        lights.Locked = true;
        m_PowerOn = false;
        WorldShakeManager.Instance.Shake(1.0f, 1.0f);
        PlayerState player_state = PlayerState.Instance;
        --player_state.Health;
        shipAudioSource.clip = damageBonk;
        shipAudioSource.Play();
        SetCurrentGear(MovementGear.STOP);
        m_Animator.SetTrigger(c_HitAnimTriggerID);

        if (crack_material)
        {
            crack_material.SetFloat(c_CrackOverlayAmountID, 1.0f - Mathf.Max(1, player_state.Health) / (float)player_state.MaxHealth);
        }

        OnTakeHit?.Invoke();

        Debug.Log("Player Hit");
    }

    private void OnLightsToggled(bool toggle)
    {
        if (PowerOn && toggle)
        {
            shipAudioSource.PlayOneShot(powerUpAudio);
            SetCurrentGear(MovementGear.SLOW);
        }
        else
        {
            shipAudioSource.PlayOneShot(powerDownAudio);
            SetCurrentGear(MovementGear.STOP);
        }
    }

    private void OnHitAnimationCompleted()
    {
        lights.ToggleLights(false);
        StartCoroutine(WaitForPowerOn());
    }

    IEnumerator WaitForPowerOn()
    {
        if (PlayerState.Instance.Health > 0)
        {
            yield return new WaitForSeconds(m_HitDisableTime);
            m_PowerOn = true;
            lights.Locked = false;
            lights.ToggleLights(true);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        scraping = true;
        if (hasCollidedThisFrame == false)
        {
            hasCollidedThisFrame = true;
            float collision_intensity = Mathf.Clamp(-Vector3.Dot(collision.contacts[0].normal, currentSpeed.normalized), 0.0f, 1.0f) * currentSpeed.magnitude;
            if (collision_intensity > collisionHardThreshold)
            {
                shipAudioSource.clip = bigBonk;
                HunterBehaviour.Instance?.OnTerrainBump(true);
            }
            else
            {
                shipAudioSource.clip = smallBonk;
                HunterBehaviour.Instance?.OnTerrainBump(false);
            }

            var addedVelocity = (collision_intensity * collisionElasticity) * collision.contacts[0].normal;
            //currentSpeed *= (currentSpeed.magnitude / addedVelocity.magnitude);
            AddImpulse(addedVelocity);

            shipAudioSource.Play();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        scraping = false;
    }

    private void OnDestroy()
    {
        lights.OnLightsToggled -= OnLightsToggled;
    }
}
