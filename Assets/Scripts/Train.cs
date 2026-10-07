using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using Unity.Mathematics;

public class Train : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private float speed = 1;


    private InputActionMap actionMap;
    private InputAction controlsAction;

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
    [SerializeField] private Transform trainHead;
    [SerializeField] private GameObject rails;

    private void Awake()
    {
        actionMap = inputActions.FindActionMap("Train");

        controlsAction = actionMap.FindAction("Controls");

        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
        
        spline = splineContainer[0];

        trainParts = GetComponentsInChildren<Transform>();

        camList = GetComponentsInChildren<Camera>();
    }

    private void Start()
    {
        for (int i = 0; i < 1000; i++)
        {
            spline.Evaluate(i/1000.0f, out float3 p, out float3 t, out float3 uV);
            Debug.Log($"{i}, {p}");
            Instantiate(rails, p, Quaternion.LookRotation(t, uV));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (actionMap.enabled) 
        { 
            float controls = controlsAction.ReadValue<float>();

            t += controls * speed * Time.deltaTime;
        }

        int j = 0;
        for (int i = 0; i < gameObject.GetComponentsInChildren<Transform>().Length; i++) 
        {
            if (trainParts[i].name.StartsWith("TrainMesh")) {
                spline.Evaluate((t+spaceBetweenParts*j) % 1, out float3 position, out float3 tangent, out float3 upVector);
                float3 offset = transform.up * (transform.localScale.y / 2);
                position +=  offset;
                trainParts[i].position = position;

                trainParts[i].rotation = UnityEngine.Quaternion.LookRotation(tangent, upVector);
                j++;
            }
        }
        transform.position = trainHead.position;
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
                for (int i = 0; i < camList.Length-1; i++) 
                {
                    if (i == camCurrentID) 
                    {
                        camList[i].GetComponent<Camera>().enabled = true;
                    }
                    else 
                    {
                        camList[i].GetComponent<Camera>().enabled = false;
                    }
                }
            }
            timerCam += Time.deltaTime;
        }

    }
}
