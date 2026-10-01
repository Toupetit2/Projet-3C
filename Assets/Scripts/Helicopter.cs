using UnityEngine;
using UnityEngine.InputSystem;

public class Helicopter : MonoBehaviour
{

    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap actionMap;
    private InputAction horizontalAction;
    private InputAction verticalAction;
    private InputAction rotateAction;

    private Rigidbody rb;

    [SerializeField] private float horizontalSpeed = 100;
    [SerializeField] private float verticalSpeed = 100;
    [SerializeField] private float rotationSpeed = 100;

    private Quaternion yRotation = Quaternion.identity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        actionMap = inputActions.FindActionMap("Helicopter");
        horizontalAction = actionMap.FindAction("HorizontalMovement");
        verticalAction = actionMap.FindAction("VerticalMovement");
        rotateAction = actionMap.FindAction("Rotate");

        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector2 horizontalInput = horizontalAction.ReadValue<Vector2>() * horizontalSpeed;
        float verticalInput = verticalAction.ReadValue<float>() * verticalSpeed;

        Vector3 force = horizontalInput.x * transform.right + horizontalInput.y * transform.forward + verticalInput * transform.up;

        rb.AddForce(force, ForceMode.Force);

        float rotationInput = rotateAction.ReadValue<float>() * rotationSpeed;
        yRotation *= Quaternion.Euler(0, rotationInput, 0);
        rb.MoveRotation(yRotation);
    }
}
