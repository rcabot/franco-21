using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class SubmarineController : MonoBehaviour
{
    public float acceleration = 7.5f;
    public Camera playerCamera;
    public Transform submarineCockpit;
    public float baseLookSpeed = 2.0f;
    public float maxLookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    public float strafeFactor = 0.0f;
    public float maxSpeed = 10.0f;

    public AnimationCurve frictionCurve;

    public float baseFriction = 0.1f;
    public float lookSmoothTime;

    public bool enableGradualRotation = false;

    Rigidbody characterController;
    Vector2 targetLookRotation;
    Vector2 currentLookRotation;
    Vector3 currentSpeed = Vector3.zero;
    Vector2 currentLookVelocity;
    AudioSource engineSound;
    AudioSource underwaterSound;
    public float speedScale;

    void Start()
    {
        characterController = GetComponent<Rigidbody>();
        engineSound = transform.Find("ship_engine").GetComponent<AudioSource>();
        underwaterSound = transform.Find("water_ambience").GetComponent<AudioSource>();
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        UpdateLookDirection();
    }

    private void FixedUpdate()
    {
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        Vector3 forward = submarineCockpit.transform.forward;
        Vector3 right = submarineCockpit.transform.right;
        float accelX = acceleration * Input.GetAxis("Vertical");
        float accelY = acceleration * Input.GetAxis("Horizontal");

        // acceleration
        currentSpeed += ((forward * accelX) + (right * accelY) * strafeFactor) * Time.deltaTime;

        // modify friction with speed
        var friction = frictionCurve.Evaluate(currentSpeed.magnitude/maxSpeed) * baseFriction * Time.deltaTime;

        // apply friction
        if (currentSpeed.magnitude > 0) 
            currentSpeed = currentSpeed.normalized * Mathf.Max(0f,currentSpeed.magnitude - friction);

        // clamp speed
        currentSpeed = Vector3.ClampMagnitude(currentSpeed, maxSpeed);

        engineSound.pitch = 1 + (4 * (currentSpeed.magnitude / maxSpeed));
        underwaterSound.pitch = 1 + (1 * (currentSpeed.magnitude / maxSpeed));
        // todo: buoyancy
        /*if (Input.GetButton("Jump"))
        {
        }
        if (Input.GetButton("Crouch"))
        {
        }*/

        // Move the controller
        characterController.MovePosition(characterController.position + currentSpeed * Time.deltaTime * speedScale);
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
}
