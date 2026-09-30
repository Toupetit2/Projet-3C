using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Move Parameters")]
    [SerializeField] private float moveSpeed = 500;
    [SerializeField, Tooltip("Additional speed, total run speed is moveSpeed + runSpeed.")] private float runSpeed = 300;
    [SerializeField] private AnimationCurve runCurve;
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private float jumpForce = 10;
    [SerializeField] private float jumpDuration = 1.5f;
    [SerializeField] private float airControlPower = 0.2f;
    [SerializeField] private float additionalGravity = 20;

    [Header("Look Parameters")]
    [SerializeField] private Transform camHolder;
    [SerializeField] private Transform originTransform;
    [SerializeField] private float sensitivity = 100;


    private InputActionMap actionMap;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction runAction;
    private InputAction lookAction;
    private InputAction interactAction;

    private Rigidbody rb;
    private CapsuleCollider coll;

    private Vector3 moveVector;
    private float curSpeed;
    private float runTimer = 0;
    private float jumpTimer = 0;
    private bool isGrounded = false;
    private bool jumping = false;
    private Quaternion yRotation = Quaternion.identity;

    private float curPitch = 0;

    private Camera cam;

    void Awake()
    {
        actionMap = inputActions.FindActionMap("FirstPerson");
        actionMap.Enable();

        moveAction = actionMap.FindAction("Move");
        jumpAction = actionMap.FindAction("Jump");
        runAction = actionMap.FindAction("Run");
        lookAction = actionMap.FindAction("Look");
        interactAction = actionMap.FindAction("Interact");

        rb = GetComponent<Rigidbody>();
        coll = GetComponent<CapsuleCollider>();

        curSpeed = moveSpeed;

        cam = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (actionMap.enabled) {
            // Set IsGrounded
            isGrounded = false;
            if (Physics.SphereCast(originTransform.position, coll.radius, -transform.up, out RaycastHit hit, (coll.height * 0.5f) - coll.radius + 0.1f, LayerMask.GetMask("Ground"))) 
            {
                isGrounded = true;
            }

            // Movement
            Vector2 moveValue = moveAction.ReadValue<Vector2>();
            moveVector = transform.forward * moveValue.y + transform.right * moveValue.x;

            // Run
            if (runAction.IsPressed()) 
            {
                curSpeed = moveSpeed + runCurve.Evaluate(runTimer) * runSpeed;

                runTimer += Time.deltaTime;
            }
            else 
            {
                runTimer = 0;
                curSpeed = moveSpeed;
            }

            // Jump
            if (jumpAction.WasPressedThisFrame() && isGrounded) 
            {
                jumping = true;
            }

            // Mouse Look
            Vector3 mouseDelta = lookAction.ReadValue<Vector2>() * sensitivity;
            yRotation *= Quaternion.Euler(0, (mouseDelta.x / Screen.width) * Time.deltaTime, 0);

            curPitch  += (-mouseDelta.y / Screen.height) * Time.deltaTime;
            curPitch = Mathf.Clamp(curPitch, -90, 90);

            camHolder.localRotation = Quaternion.Euler(curPitch, 0, 0);

            // Interact
            if (interactAction.WasPressedThisFrame()) 
            {
                float detectionRadius = 3;
                Collider[] hitColliders = Physics.OverlapSphere(originTransform.position, detectionRadius, LayerMask.GetMask("Vehicle"));

                if (hitColliders.Length > 0)
                {
                    Collider firstHit = hitColliders[0];

                    Vehicle vehicle = firstHit.GetComponent<Vehicle>();

                    vehicle.getActionMap().Enable();
                    actionMap.Disable();

                    vehicle.getCamera().enabled = true;
                    cam.enabled = false;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (actionMap.enabled) {
            rb.MoveRotation(yRotation);

            if (isGrounded) 
            {
                rb.linearVelocity = moveVector * curSpeed * Time.deltaTime;
            }
            else 
            {
                rb.linearVelocity = moveVector * curSpeed * Time.deltaTime * airControlPower;
            }
            if (jumping) 
            {
                float jumpVel = jumpCurve.Evaluate(jumpTimer / jumpDuration) * jumpForce * Time.deltaTime;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVel, rb.linearVelocity.z);

                jumpTimer += Time.deltaTime;

                if (jumpTimer > jumpDuration)
                {
                    jumpTimer = 0;
                    jumping = false;
                }
            }
            else 
            {
                rb.AddForce(Vector3.down * additionalGravity, ForceMode.Force);
            }
        }
    }
}
