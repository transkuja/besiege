using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VehicleController : MonoBehaviour {
    Rigidbody rb;
    float maxSpeed = 20.0f;
    float speed = 1.0f;
    public CinemachineFreeLook freelookCamera;
    bool isInitialized = false;

    void Start () {
        rb = GetComponent<Rigidbody>();
    }

    public void InitController(CinemachineFreeLook _cameraRef)
    {
        freelookCamera = _cameraRef;
        freelookCamera.Follow = transform;
        freelookCamera.LookAt = transform;
        isInitialized = true;
    }

    void FixedUpdate () {
        if (!GameState.isInCreatorMode && isInitialized)
        {
            float oldVelocityY = rb.velocity.y;
            rb.AddForce((Input.GetAxisRaw("Vertical") * transform.forward) * speed);
            Vector3 newVelocity = Vector3.ClampMagnitude(new Vector3(rb.velocity.x, 0, rb.velocity.z), maxSpeed);
            rb.velocity = newVelocity + oldVelocityY * Vector3.up;

            rb.AddTorque(Input.GetAxisRaw("Horizontal") * transform.up * speed);
            rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, 10);
        }
    }
}
