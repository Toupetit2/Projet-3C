using NUnit.Framework;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class Vehicle : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private InputActionAsset inputActions;
    public enum VehicleTypes { Car, Plane };

    public VehicleTypes vehicleType;

    private InputActionMap actionMap;
    private InputActionMap playerActionMap;
    private InputAction interactAction;


    void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Vehicle");

        actionMap = inputActions.FindActionMap(vehicleType.ToString());
        playerActionMap = inputActions.FindActionMap("FirstPerson");

        interactAction = actionMap.FindAction("Interact");
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
            }
        }

    }

    public InputActionMap getActionMap() 
    {
        return actionMap;
    }
}
