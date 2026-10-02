using NUnit.Framework;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Vehicle : MonoBehaviour
{
    [Header("General")]
    [SerializeField] public InputActionAsset inputActions;
    public enum VehicleTypes { Car, Helicopter };

    public VehicleTypes vehicleType;

    private InputActionMap actionMap;
    private InputActionMap playerActionMap;
    private InputAction interactAction;


    [SerializeField] private GameObject player;
    private Camera playerCam;
    private Camera cam;

    [SerializeField] private Vector3 respawnPositionOffset;


    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Vehicle");

        actionMap = inputActions.FindActionMap(vehicleType.ToString());
        playerActionMap = inputActions.FindActionMap("FirstPerson");

        interactAction = actionMap.FindAction("Interact");

        cam = gameObject.GetComponentInChildren<Camera>();

        playerCam = player.GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (actionMap.enabled) 
        {
            if (interactAction.IsPressed())
            {
                actionMap.Disable();
                playerActionMap.Enable();

                cam.enabled = false;
                playerCam.enabled = true;
                
                player.SetActive(true);
                player.transform.position = transform.position + respawnPositionOffset;
            }
        }
    }

    public InputActionMap getActionMap() 
    {
        return actionMap;
    }

    public Camera getCamera()
    {
        return cam;
    }
}
