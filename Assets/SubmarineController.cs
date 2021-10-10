using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private AudioSource shipAlarmAudioSource;

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

    private int intro_fall = 2;
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

    [SerializeField, Header("Sonar")]
    AudioSource sonarPingSound = default;

    [SerializeField, RangeBeginEnd(0f, 1f)]
    RangeFloat sonarPingVolumeRange = new RangeFloat(0f, 0.25f);

    [SerializeField, RangeBeginEnd(0f, 3f)]
    RangeFloat sonarPingPitchRange = new RangeFloat(1f, 1.5f);

    [SerializeField, Range(0f, 1000f)]
    float sonarCreatureMaxDistance = 150f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference gearShiftUpAction;
    [SerializeField] private InputActionReference gearShiftDownAction;

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
        shipAlarmAudioSource = transform.Find("ship_alarm").GetComponent<AudioSource>();
        shipAudioSource = this.RequireComponent<AudioSource>();

        var introCollisionBox = transform.Find("intro_finish_trigger").GetComponent<intro_collision_event_handler>();
        introCollisionBox.EventCollidedWithGround += OnIntroCollisionEnter;
    }

    void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentGear = MovementGear.SLOW;
        acceleration = gearSpeeds[(int)currentGear];

        gearShiftUpAction.action.performed += OnGearShiftUpPressed;
        gearShiftDownAction.action.performed += OnGearShiftDownPressed;

        intro_fall = 2;
        lights.Locked = true;
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

        if( (LightsOn && m_PowerOn) && engineSound.isPlaying == false)
        {
            engineSound.Play();
            cabinAmbience.Play();
            sonarPingSound.Play();
        }
        else if( LightsOn == false || m_PowerOn == false)
        {
            engineSound.Stop();
            sonarPingSound.Stop();
            cabinAmbience.Stop();
        }

        UpdateLookDirection();
    }

    private void FixedUpdate()
    {
        if( intro_fall == 2 )
        {
            currentSpeed = new Vector3(0.0f, -2.0f, 0.0f);
        }
        else if (intro_fall == 1)
        {
            currentSpeed += new Vector3(0.0f, 0.5f * Time.fixedDeltaTime, 0.0f);
            if( currentSpeed.y >= -0.1f)
            {
                currentSpeed = new Vector3(0.0f, 0.0f, 0.0f);
                lights.Locked = false;
                lights.ToggleLights(true);
                currentSpeed = Vector3.zero;
                intro_fall = 0;
            }
        }

        UpdateMovement();
        UpdateSonar();
        hasCollidedThisFrame = false;
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

        Vector2 move_input = LightsOn ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        Vector3 strafe_acceleration = right * move_input.x * strafeFactor;
        Vector3 forward_acceleration = forward * move_input.y;

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
        Vector2 look_input = lookAction.action.ReadValue<Vector2>();

        targetLookRotation.x += -look_input.y * baseLookSpeed;
        targetLookRotation.x = Mathf.Clamp(targetLookRotation.x, -lookXLimit, lookXLimit);
        targetLookRotation.y += look_input.x * baseLookSpeed;
        var prevLook = targetLookRotation - currentLookRotation;
        currentLookRotation = Vector2.SmoothDamp(currentLookRotation, targetLookRotation, ref currentLookVelocity, lookSmoothTime,maxLookSpeed);
        if (enableGradualRotation)
        {
            targetLookRotation = currentLookRotation + prevLook;
        }
        playerCamera.transform.rotation = Quaternion.Euler(targetLookRotation.x, targetLookRotation.y, 0);
        submarineCockpit.localRotation = Quaternion.Euler(currentLookRotation.x, currentLookRotation.y, 0);
    }

    private void UpdateSonar()
    {
        HunterBehaviour creature = HunterBehaviour.Instance;
        if (creature == null || sonarPingSound == null)
            return;

        Vector3 to_creature = creature.transform.position - transform.position;
        float sqr_distance = to_creature.sqrMagnitude;
        if (sqr_distance < (sonarCreatureMaxDistance * sonarCreatureMaxDistance))
        {
            float actual_distance = Mathf.Sqrt(sqr_distance);
            float normalised_distance = 1.0f - actual_distance / sonarCreatureMaxDistance;
            sonarPingSound.pitch = sonarPingPitchRange.SampleNormalised(normalised_distance);
            sonarPingSound.volume = sonarPingVolumeRange.SampleNormalised(normalised_distance);
        }
        else
        {
            sonarPingSound.pitch = sonarPingPitchRange.start;
            sonarPingSound.volume = sonarPingVolumeRange.start;
        }
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

    private void OnHitAnimationStarted()
    {
        shipAlarmAudioSource.Play();
    }
    private void OnHitAnimationCompleted()
    {
        shipAlarmAudioSource.Stop();
        lights.ToggleLights(false);
        StartCoroutine(WaitForPowerOn());
    }

    IEnumerator WaitForPowerOn()
    {
        yield return new WaitForSeconds(m_HitDisableTime);

        if (PlayerState.Instance.Health > 0)
        {
            m_PowerOn = true;
            lights.Locked = false;
            lights.ToggleLights(true);
        }
    }

    private void OnIntroCollisionEnter(Collider other)
    {
        if (intro_fall == 2)
        {
            if (other as TerrainCollider)
            {
                intro_fall = 1;
                return;
            }
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
        gearShiftUpAction.action.performed -= OnGearShiftUpPressed;
        gearShiftDownAction.action.performed -= OnGearShiftDownPressed;
    }

    #region Input Listeners

    private void OnGearShiftDownPressed(InputAction.CallbackContext obj)
    {
        if (m_PowerOn && LightsOn)
        {
            int gear = ((int)currentGear - 1 + num_gears) % num_gears;
            SetCurrentGear((MovementGear)gear);
        }
    }

    private void OnGearShiftUpPressed(InputAction.CallbackContext obj)
    {
        if (m_PowerOn && LightsOn)
        {
            int gear = ((int)currentGear + 1) % num_gears;
            SetCurrentGear((MovementGear)gear);
        }
    }

    #endregion
}
