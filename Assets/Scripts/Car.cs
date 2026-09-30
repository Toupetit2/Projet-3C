using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Car : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap actionMap;
    private InputAction accelerateAction;
    private InputAction brakeAction;
    private InputAction steerAction;
    private InputAction reverseAction;

    private Rigidbody rb;

    private Quaternion rotation;

    // Engine

    private int maxRpm = 7000;
    private int a = 250;
    private int b = 50;
    private float c = 1;
    private float d = 0.57f;
    private float f = 0.30f;

    private float wheelRadius = 0.3f;

    private Dictionary<int, float> gears = new Dictionary<int, float>()
    {
        {-1, -3.3f },
        {0, 0f },
        {1, 3.2f },
        {2, 2.0f },
        {3, 1.4f },
        {4, 1.05f },
        {5, 0.85f }
    };

    private int gear = 1;

    private float differential_ratio = 4.1f;

    [SerializeField] private bool automaticTransmissionActive = true;

    Camera cam;
    [Header ("Camera Settings")]
    [SerializeField] private Vector3 camOffset;
    [SerializeField] private Vector2 camSmoothing;



    private void Awake()
    {
        actionMap = inputActions.FindActionMap("Car");

        accelerateAction = actionMap.FindAction("Accelerate");
        brakeAction = actionMap.FindAction("Brake");
        steerAction = actionMap.FindAction("Steer");
        reverseAction = actionMap.FindAction("Reverse");

        rb = GetComponent<Rigidbody>();

        rotation = rb.rotation;

        cam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (actionMap.enabled) 
        {
            if (brakeAction.IsPressed()) 
            {
                Debug.Log("Brake");
            }
            if (reverseAction.IsPressed()) 
            {
                Debug.Log("Reverse");
            }

           
            rotation *= Quaternion.Euler(steerAction.ReadValue<float>() * 0.01f * Vector3.Dot(rb.linearVelocity, transform.forward), 0, 0);
        }

        // Camera

        Vector3 camObjective = transform.position + gameObject.transform.rotation * camOffset;
        float camPosX = Mathf.Lerp(cam.transform.position.x, camObjective.x, 0.0001f * Time.deltaTime);
        float camPosY = Mathf.Lerp(cam.transform.position.y, camObjective.y, 0.0001f * Time.deltaTime);
        float camPosZ = Mathf.Lerp(cam.transform.position.z, camObjective.z, 0.0001f * Time.deltaTime);

        cam.transform.position = new Vector3(camPosX, camPosY, camPosZ);

        Debug.Log($"{cam.transform.position}, {camObjective}");
        
    }

    private void FixedUpdate()
    {
        simulateEngine();

        rb.MoveRotation(rotation.normalized);
    }

    private void simulateEngine() 
    {
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

        float wheelRpm = forwardSpeed / (2 * Mathf.PI * wheelRadius) * 60f;

        float engineRpm = wheelRpm * gears[gear] * differential_ratio;

        if (automaticTransmissionActive) 
        {
            automaticTransmission(engineRpm);

            engineRpm = wheelRpm * gears[gear] * differential_ratio;
        }

        float torque = 0;
        if (engineRpm < maxRpm) 
        {
            float x = engineRpm / maxRpm;

            torque = (a - b) * Mathf.Exp(-Mathf.Pow((c * (x - d)), 2) / (f * f)) + b;
        }

        float throttle = accelerateAction.ReadValue<float>();

        float engineTorque = torque * throttle;

        float wheelTorque = engineTorque * gears[gear] * differential_ratio;

        float wheelForce = wheelTorque / wheelRadius;

        float acceleration = wheelForce / rb.mass;

        rb.AddForce(transform.forward * wheelForce, ForceMode.Force);

        Debug.Log($"Speed:{forwardSpeed:F2}m/s, Gear: {gear}");
    }

    private void automaticTransmission(float engineRpm) 
    {
        if (gear < gears.Keys.ElementAt(gears.Count - 1) && engineRpm > 5000)
        {
            gear += 1;
        }
        else if (gear > 1 && engineRpm < 2500)
        {
            gear -= 1;
        }
    }
}
