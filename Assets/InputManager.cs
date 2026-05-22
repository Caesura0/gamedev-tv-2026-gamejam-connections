using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions inputActions;


    // Read your movement input as a Vector2 (x for horizontal, y for vertical)
    public Vector2 Movement { get; private set; }
    public bool InteractHeld { get; private set; }
    public bool MoveButtonPushed {  get; private set; }


    // ========= EVENTS =========
    // Triggered when a relevent button is pressed, access by subscribing to these event in other scripts.
    //For example, in a PlayerController script, you could subscribe to OnInteractPressed to handle interaction logic when the player presses the interact button

    public event Action OnInteractStarted;
    public event Action OnInteractPressed; 
    public event Action OnSecondaryInteractPressed;
    public event Action OnMenuPressed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Enable();

        // Movement
        inputActions.Player.Movement.performed += OnMovementPerformed;
        inputActions.Player.Movement.canceled += OnMovementCanceled;

        // Interact
        inputActions.Player.Interact.performed += OnInteract;
        inputActions.Player.Interact.started += OnInteractPressedThisFrame;
        inputActions.Player.Interact.started += OnInteractHeld;
        inputActions.Player.Interact.canceled += OnInteractHeldCancel;

        // Secondary Interact
        inputActions.Player.SecondaryInteract.performed += OnSecondaryInteract;

        // Menu
        inputActions.Player.Menu.performed += OnMenu;
    }

    private void OnDisable()
    {
        if(inputActions == null) return; // In case OnDisable is called before Awake
        // Movement
        inputActions.Player.Movement.performed -= OnMovementPerformed;
        inputActions.Player.Movement.canceled -= OnMovementCanceled;

        // Interact
        inputActions.Player.Interact.performed -= OnInteract;
        inputActions.Player.Interact.started -= OnInteractPressedThisFrame;
        inputActions.Player.Interact.started -= OnInteractHeld;
        inputActions.Player.Interact.canceled -= OnInteractHeldCancel;

        // Secondary Interact
        inputActions.Player.SecondaryInteract.performed -= OnSecondaryInteract;

        // Menu
        inputActions.Player.Menu.performed -= OnMenu;

        inputActions.Disable();
    }

    // This sets the public movement property for reading.

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        MoveButtonPushed = true;
        Movement = context.ReadValue<Vector2>();
    }

    private void OnMovementCanceled(InputAction.CallbackContext context)
    {
        MoveButtonPushed = false;
        Movement = Vector2.zero;
    }

    // ========= BUTTONS =========

    private void OnInteract(InputAction.CallbackContext context)
    {
        OnInteractPressed?.Invoke();
    }

    private void OnInteractPressedThisFrame(InputAction.CallbackContext context)
    {
        OnInteractStarted?.Invoke(); 
    }

    // These set the public InteractHeld property for reading.

    private void OnInteractHeld(InputAction.CallbackContext context)
    {
        InteractHeld = true;
    }
    private void OnInteractHeldCancel(InputAction.CallbackContext context)
    {
        InteractHeld = false;

    }

    // Likely Won't need
    private void OnSecondaryInteract(InputAction.CallbackContext context)
    {
        OnSecondaryInteractPressed?.Invoke();
    }

    private void OnMenu(InputAction.CallbackContext context)
    {
        OnMenuPressed?.Invoke();
    }
}