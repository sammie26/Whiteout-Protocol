using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class Move : MonoBehaviour
{

    public float liftForce = 15f; //Public, to change later if required.
    float moveForce = 15f;
    float rotationTorque = 0.05f;
    float pitchTorque = 1f;
    float stabilizationTorque = 0.2f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void HandleThrust()
    {
        if (Input.GetKey(KeyCode.N))
        {
            if (liftForce - Time.fixedDeltaTime > 5)
                liftForce -= Time.fixedDeltaTime * 2.0f;
            else
                liftForce = 5.0f;

        }
        if (Input.GetKey(KeyCode.M))
        {
            if (liftForce + Time.fixedDeltaTime < 25)
                liftForce += Time.fixedDeltaTime * 2.0f;
            else
                liftForce = 25.0f;
        }

    }

    void FixedUpdate()
    {
        HandleThrust();
        HandleLift();
        HandleMovement();
        HandleRotation();
        StabilizeDrone();
    }

    void HandleLift()
    {
        if (Input.GetKey(KeyCode.Space))
            rb.AddForce(transform.up * liftForce, ForceMode.Force);
    }

    void HandleMovement()
    {
        if (!Input.GetKey(KeyCode.Space))
            return;

        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) 
            moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S)) 
            moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A)) 
            moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D)) 
            moveDirection += transform.right;
        rb.AddForce(moveDirection.normalized * moveForce, ForceMode.Force);
    }

    void HandleRotation()
    {
        float yaw = 0f;
        float pitch = 0f;
        if (Input.GetKey(KeyCode.Q)) 
            yaw = -1f;
        if (Input.GetKey(KeyCode.E)) 
            yaw = 1f;

        rb.AddTorque(transform.up * yaw * rotationTorque, ForceMode.Force);

        if (Input.GetKey(KeyCode.Z))
            pitch = -1f;
        if (Input.GetKey(KeyCode.C))
            pitch = 1f;

        rb.AddTorque(transform.right * pitch * pitchTorque, ForceMode.Force); 
    }

    void StabilizeDrone()
    {
        Vector3 localTilt = transform.localRotation.eulerAngles;
        if (localTilt.x > 180) 
            localTilt.x -= 360;
        if (localTilt.z > 180) 
            localTilt.z -= 360;

        Vector3 stabilizationTorqueVec = new Vector3(-localTilt.x, 0f, -localTilt.z) * stabilizationTorque;
        rb.AddTorque(transform.TransformVector(stabilizationTorqueVec), ForceMode.Force);
    }
}
