﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VehicleController : MonoBehaviour {
    Rigidbody rb;
    float maxSpeed = 20.0f;
    public float speed = 20.0f;
    public float angularSpeed = 5.0f;
    public CinemachineFreeLook freelookCamera;
    bool isInitialized = false;
    bool canMove = false;

    void Start () {
        rb = GetComponent<Rigidbody>();
        if (GetComponentsInChildren<ZeroGBlock>().Length > 0)
            canMove = true;
    }

    public void InitController(CinemachineFreeLook _cameraRef)
    {
        freelookCamera = _cameraRef;
        freelookCamera.Follow = transform;
        freelookCamera.LookAt = transform;
        isInitialized = true;
    }

    void FixedUpdate () {
        if (!GameState.isInCreatorMode && isInitialized && canMove)
        {
            float oldVelocityY = rb.velocity.y;
            rb.AddForce((Input.GetAxisRaw("Vertical") * transform.forward) * speed);
            Vector3 newVelocity = Vector3.ClampMagnitude(new Vector3(rb.velocity.x, 0, rb.velocity.z), maxSpeed);
            rb.velocity = newVelocity + oldVelocityY * Vector3.up;


            if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f)
            {
                rb.AddTorque(Input.GetAxisRaw("Horizontal") * transform.up * angularSpeed);
                rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, 5);
            }
            else
                rb.angularVelocity = Vector3.zero;

            freelookCamera.m_RecenterToTargetHeading.m_enabled = true;
        }
    }
}
