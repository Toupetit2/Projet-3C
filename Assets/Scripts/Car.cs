using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private float engineMultiplier = 0.8f;
    [SerializeField] private bool automaticTransmissionActive = true;
    
    [SerializeField] private float maxBrake = 100f;
    
    
    // Wheels
    [SerializeField] private float maxSteeringAngle = 35.0f;
    [SerializeField, Tooltip("Multiply the force of the opposite direction of the slide")] private int lateralGripForce = 3500;

    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;
    [SerializeField] private Transform backLeftWheel;
    [SerializeField] private Transform backRightWheel;

    private List<Transform> wheels;

    private float B = 3f;
    private float C = 1.3f;
    private float D = 1f;
    private float E = 0.5f;

    [SerializeField] private float maxSteeringByUpdate = 1f;

    private float steeringAngle;

    [SerializeField] private float centerWheelSteeringMultiplier = 0.8f;

    [Header ("Camera Settings")]
    private Camera cam;
    [SerializeField] private Vector3 camOffset;
    [SerializeField] private Vector3 camSmoothing;
    [SerializeField] private float camSmoothingRotation;
    [SerializeField] private int camBaseFOV = 60;
    [SerializeField] private int camMaxFOV = 80;
    [SerializeField] private float camMaxFOVSpeed = 100.0f;
    private Quaternion camRotationOffset = Quaternion.Euler(20, 0, 0);

    [Header("Color Settings")]
    [SerializeField] private List<Color> gearColors;
    private MeshRenderer meshRenderer;

    private void Awake()
    {
        actionMap = inputActions.FindActionMap("Car");

        accelerateAction = actionMap.FindAction("Accelerate");
        brakeAction = actionMap.FindAction("Brake");
        steerAction = actionMap.FindAction("Steer");
        reverseAction = actionMap.FindAction("Reverse");

        rb = GetComponent<Rigidbody>();

        cam = GetComponentInChildren<Camera>();

        wheels = new List<Transform>{frontLeftWheel, frontRightWheel, backLeftWheel, backRightWheel};

        meshRenderer = transform.Find("CarMesh").GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        cam.transform.SetParent(transform.parent, false);
    }

    // Update is called once per frame
    void Update()
    {
        // Camera

        Vector3 camObjective = transform.position + transform.rotation * camOffset;
        Quaternion rotationObjective = transform.rotation * camRotationOffset;
        float camPosX = Mathf.Lerp(cam.transform.position.x, camObjective.x, camSmoothing.x * Time.deltaTime);
        float camPosY = Mathf.Lerp(cam.transform.position.y, camObjective.y, camSmoothing.y * Time.deltaTime);
        float camPosZ = Mathf.Lerp(cam.transform.position.z, camObjective.z, camSmoothing.z * Time.deltaTime);

        cam.transform.localPosition = new Vector3(camPosX, camPosY, camPosZ);

        Quaternion camRotation = Quaternion.Slerp(cam.transform.rotation, rotationObjective, camSmoothingRotation * Time.deltaTime);

        cam.transform.rotation = camRotation;


    }

    private void FixedUpdate()
    {
        simulateEngine();
    }

    private void simulateEngine() 
    {
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);

        float wheelRpm = (forwardSpeed / (2 * Mathf.PI * wheelRadius) * 60f);

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

        float throttle;
        if (gear > 0)
        {
            throttle = accelerateAction.ReadValue<float>();
        }
        else 
        {
            throttle = reverseAction.ReadValue<float>();
        }

        float engineTorque = torque * throttle * engineMultiplier;

        float wheelTorque = engineTorque * gears[gear] * differential_ratio;

        float wheelForce = wheelTorque / wheelRadius;

        Vector3 drivingForce = backLeftWheel.forward * wheelForce;

        rb.AddForceAtPosition(drivingForce, backLeftWheel.position);
        rb.AddForceAtPosition(drivingForce, backRightWheel.position);

        //Debug.Log($"Speed:{forwardSpeed:F2}m/s, Gear: {gear}");


        // Steering
        float steerInput = steerAction.ReadValue<float>();
        float objectiveAngle = Mathf.Clamp(steeringAngle + steerInput * maxSteeringByUpdate, -maxSteeringAngle, maxSteeringAngle);
        if (steerInput == 0)
        {
            objectiveAngle *= centerWheelSteeringMultiplier * Time.deltaTime;
        }
        steeringAngle = steeringAngle + Mathf.Clamp(objectiveAngle- steeringAngle, -maxSteeringByUpdate, maxSteeringByUpdate);


        frontLeftWheel.localRotation = Quaternion.Euler(0f, steeringAngle, 0f);

        frontRightWheel.localRotation = Quaternion.Euler(0f, steeringAngle, 0f);

        foreach (Transform wheel in wheels) 
        {
            Vector3 wheelPosition = wheel.position;
            Vector3 wheelForward = wheel.forward;
            Vector3 wheelRight = wheel.right;

            Vector3 velocity = rb.GetPointVelocity(wheelPosition);

            float forwardVelocity = Vector3.Dot(velocity, wheelForward);
            float sidewaysVelocity = Vector3.Dot(velocity, wheelRight);

            float slipAngle = Mathf.Atan2(sidewaysVelocity, Mathf.Abs(forwardVelocity));

            if (Mathf.Abs(forwardVelocity) < 0.1f) 
            {
                slipAngle = 0;
            }

            float tireForce = D * Mathf.Sin(C * Mathf.Atan(B * slipAngle - E * (B * slipAngle - Mathf.Atan(B * slipAngle))));

            //wheel.GetComponent<Renderer>().material.color = new Color(tireForce, 0, 0);

            float lateralForcePower = -tireForce * lateralGripForce;

            //float lateralForcePower = -sidewaysVelocity * lateralGripForce;
            Vector3 lateralForce = wheelRight * lateralForcePower;

            rb.AddForceAtPosition(lateralForce, wheelPosition);
        }

        // brake
        float brake = brakeAction.ReadValue<float>();

        Vector3 brakingForce = Vector3.zero;

        if (forwardSpeed > 0.1f) 
        {
            brakingForce = -transform.forward * brake * maxBrake;
        }
        else if (forwardSpeed < -0.1f)
        {
            brakingForce = transform.forward * brake * maxBrake;
        }
        
        rb.AddForce(brakingForce);

        cam.fieldOfView = Mathf.Lerp(camBaseFOV, camMaxFOV, forwardSpeed/camMaxFOVSpeed);
    }

    private void automaticTransmission(float engineRpm) 
    {
        if (gear < gears.Keys.ElementAt(gears.Count - 1) && engineRpm > 5000)
        {
            gear += 1;
            meshRenderer.material.color = gearColors[gear+1];
        }
        else if (gear > 1 && engineRpm < 2500)
        {
            gear -= 1;
            meshRenderer.material.color = gearColors[gear + 1];
        }
        if (reverseAction.IsPressed() && gear <= 1)
        {
            gear = -1;
            meshRenderer.material.color = gearColors[0];
        }
        else if (accelerateAction.IsPressed() && gear < 1)
        {
            gear = 1;
            meshRenderer.material.color = gearColors[2];
        }
    }
}
