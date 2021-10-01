using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class SubmarineController : MonoBehaviour
{
    public float acceleration = 7.5f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    public float strafeFactor = 0.0f;
    public float maxSpeed = 10.0f;

    public float friction = 0.1f;

    CharacterController characterController;
    Vector3 currentSpeed = Vector3.zero;
    float rotationX = 0;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        UpdateLookDirection();
        UpdateMovement();
    }

    private void UpdateMovement()
    {
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;
        float accelX = acceleration * Input.GetAxis("Vertical");
        float accelY = acceleration * Input.GetAxis("Horizontal");

        // acceleration
        currentSpeed += ((forward * accelX) + (right * accelY) * strafeFactor) * Time.deltaTime;

        // apply friction
        if (currentSpeed.magnitude > 0) 
            currentSpeed = currentSpeed.normalized * Mathf.Max(0f,currentSpeed.magnitude - friction * Time.deltaTime);

        // clamp speed
        currentSpeed = Vector3.ClampMagnitude(currentSpeed, maxSpeed);

        // todo: buoyancy
        /*if (Input.GetButton("Jump"))
        {
        }
        if (Input.GetButton("Crouch"))
        {
        }*/

        // Move the controller
        characterController.Move(currentSpeed);
    }

    private void UpdateLookDirection()
    {
        // Player and Camera rotation
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
    }
}
