using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class Train : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap actionMap;
    private InputAction controlsAction;
    private InputAction backToRailsAction;

    private Rigidbody rb;

    [Header("Camera")]
    private Camera cam;
    [Header("Spline")]
    [SerializeField] private SplineContainer splineContainer;
    private Spline spline;
    private float t = 0.95f; // position sur les rails


    private Transform[] trainParts;
    [SerializeField] private float spaceBetweenParts = 0.05f;

    private float timerCam = 0;
    private int camCurrentID = 0;
    private Camera[] camList;
    [Header("Others")]
    [SerializeField] private float cameraTime;
    private Transform trainHead;
    [SerializeField] private GameObject rails;

    [Header("Speed")]
    [SerializeField] private float maxSpeed = 3;
    [SerializeField] private float acceleration = 1;
    private float currentSpeed = 0;
    private float splineLength;
    private float distance;

    [SerializeField] private float derailleRatio = 3000;

    private bool deraille = false;




    private void Awake()
    {
        actionMap = inputActions.FindActionMap("Train");

        controlsAction = actionMap.FindAction("Controls");
        backToRailsAction = actionMap.FindAction("BackToRails");

        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
        
        spline = splineContainer[0];

        trainParts = GetComponentsInChildren<Transform>();

        camList = GetComponentsInChildren<Camera>();

        trainHead = trainParts[1];
    }

    private void Start()
    {
        for (int i = 0; i < 1000; i++)
        {
            spline.Evaluate(i/1000.0f, out float3 p, out float3 t, out float3 uV);
            Instantiate(rails, p, Quaternion.LookRotation(t, uV));
        }

        splineLength = spline.GetLength();
        distance = 0.99f * splineLength;
    }

    // Update is called once per frame
    void Update()
    {
        if (actionMap.enabled)
        {
            float controls;
            if (deraille)
            {
                controls = -0.5f;
            }
            else 
            {
                controls = controlsAction.ReadValue<float>();
            }
            float targetSpeed = controls * maxSpeed;

            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            distance += currentSpeed * Time.deltaTime;

            if (backToRailsAction.IsPressed()) 
            {
                deraille = false;
            }
        }

        int j = 0;
        for (int i = 0; i < gameObject.GetComponentsInChildren<Transform>().Length; i++) 
        {
            if (trainParts[i].name.StartsWith("TrainMesh")) {
                if (!deraille)
                {
                    spline.Evaluate(((distance - spaceBetweenParts * j) / splineLength) % 1, out float3 position, out float3 tangent, out float3 upVector);
                    float3 offset = transform.up * (transform.localScale.y / 2);
                    position += offset;
                    trainParts[i].position = position;

                    trainParts[i].rotation = UnityEngine.Quaternion.LookRotation(tangent, upVector);
                    j++;
                }
                else
                {
                    trainParts[i].position = trainParts[i].position + trainParts[i].forward * Mathf.Max(currentSpeed, 0) * Time.deltaTime * 1f;
                }
            }
        }
        if (!deraille)
        {
            transform.position = trainHead.position;
        }
        if (actionMap.enabled) { 
            //cam update
            if (timerCam > cameraTime) 
            {
                timerCam = 0;
                if (camCurrentID < camList.Length - 1) 
                {
                    camCurrentID++;
                }
                else 
                {
                    camCurrentID = 0;
                }
                for (int i = 0; i < camList.Length; i++) 
                {
                    if (i == camCurrentID) 
                    {
                        camList[i].GetComponent<Camera>().enabled = true;
                        camList[i].GetComponent<AudioListener>().enabled = true;
                    }
                    else
                    {
                        camList[i].GetComponent<Camera>().enabled = false;
                        camList[i].GetComponent<AudioListener>().enabled = false;
                    }
                }
            }
            timerCam += Time.deltaTime;
        }
        if (spline.EvaluateCurvature((distance / splineLength) % 1) * derailleRatio * currentSpeed > 1 && currentSpeed > 10) 
        {
            deraille = true;
        }

    }
}
