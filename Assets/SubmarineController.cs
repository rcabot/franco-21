using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class SubmarineController : MonoBehaviour
{
    public float walkingSpeed = 7.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
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
        // Press Left Shift to run
        float curSpeedX = walkingSpeed * Input.GetAxis("Vertical");
        float curSpeedY = walkingSpeed * Input.GetAxis("Horizontal");
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);
/*
        if (Input.GetButton("Jump"))
        {
            moveDirection.y = jumpSpeed;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }*/

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
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
