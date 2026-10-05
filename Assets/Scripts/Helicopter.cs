using UnityEngine;
using UnityEngine.InputSystem;

public class Helicopter : MonoBehaviour
{

    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap actionMap;
    private InputAction horizontalAction;
    private InputAction verticalAction;
    private InputAction rotateAction;
    private InputAction zoomAction;
    private InputAction lookAction;

    private Rigidbody rb;

    [SerializeField] private float horizontalSpeed = 100;
    [SerializeField] private float verticalSpeed = 100;
    [SerializeField] private float rotationSpeed = 100;
    [SerializeField] private float pivotSpeed = 0.001f;

    private Quaternion yRotation = Quaternion.identity;

    [Header("Camera")]
    private Camera cam;
    [SerializeField] private float zoomSpeed = 100;
    [SerializeField] private float sensitivity = 100;
    private Vector3 oldPosition;

    void Awake()
    {
        actionMap = inputActions.FindActionMap("Helicopter");
        horizontalAction = actionMap.FindAction("HorizontalMovement");
        verticalAction = actionMap.FindAction("VerticalMovement");
        rotateAction = actionMap.FindAction("Rotate");
        zoomAction = actionMap.FindAction("Zoom");
        lookAction = actionMap.FindAction("Look");

        rb = GetComponent<Rigidbody>();
        cam = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        cam.transform.SetParent(transform.parent, false);
    }

    // Update is called once per frame
    void Update()
    {
        cam.fieldOfView += -zoomAction.ReadValue<float>() * zoomSpeed * Time.deltaTime;

        Vector3 mouseDelta = lookAction.ReadValue<Vector2>() * sensitivity;

        cam.transform.localPosition += -mouseDelta.x * cam.transform.right + -mouseDelta.y * cam.transform.up;

        cam.transform.LookAt(transform);
    }

    private void FixedUpdate()
    {
        Vector2 horizontalInput = horizontalAction.ReadValue<Vector2>() * horizontalSpeed;
        float verticalInput = verticalAction.ReadValue<float>() * verticalSpeed;

        Vector3 force = verticalInput * transform.up;

        rb.AddForce(force, ForceMode.Force);

        yRotation *= Quaternion.Euler(horizontalInput.y*pivotSpeed, 0, -horizontalInput.x*pivotSpeed);

        float rotationInput = rotateAction.ReadValue<float>() * rotationSpeed;
        yRotation *= Quaternion.Euler(0, rotationInput, 0);
        rb.MoveRotation(yRotation);


        cam.transform.position += rb.position - oldPosition;

        oldPosition = transform.position;
    }
}
