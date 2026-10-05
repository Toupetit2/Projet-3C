using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Splines;
using UnityEngine.UIElements;

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
    private float t = 0.0f; // position sur les rails

    // Engine
    private int maxRpm = 7000;
    private int a = 250;
    private int b = 50;
    private float c = 1;
    private float d = 0.57f;
    private float f = 0.30f;


    private void Awake()
    {
        actionMap = inputActions.FindActionMap("Train");

        controlsAction = actionMap.FindAction("Controls");

        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
        spline = splineContainer[0];
    }

    // Update is called once per frame
    void Update()
    {
        float controls = controlsAction.ReadValue<float>();

        t += controls * speed * Time.deltaTime;

        spline.Evaluate(t%1, out float3 position, out float3 tangent, out float3 upVector);

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(tangent, upVector);

    }
}
