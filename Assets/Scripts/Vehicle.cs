using NUnit.Framework;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Vehicle : MonoBehaviour
{
    [Header("General")]
    [SerializeField] public InputActionAsset inputActions;
    public enum VehicleTypes { Car, Plane };

    public VehicleTypes vehicleType;

    private InputActionMap actionMap;
    private InputActionMap playerActionMap;
    private InputAction interactAction;

    [SerializeField] private Camera playerCam;
    private Camera cam;


    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Vehicle");

        actionMap = inputActions.FindActionMap(vehicleType.ToString());
        playerActionMap = inputActions.FindActionMap("FirstPerson");

        interactAction = actionMap.FindAction("Interact");

        cam = gameObject.GetComponentInChildren<Camera>();
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
