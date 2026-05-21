using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] private float stepDelay = 0.15f;
    [SerializeField] private float movementSpeed = 10f;

    private GridManager gridManager;
    private Vector2Int currentGridPosition;
    private DirectionEnum lastMoveDirection;

    private Vector3 targetWorldPosition;
    private float stepCooldownTimer;
    private bool isWalking;
    private bool isHoldingInteract;
    private bool isPressingMove;

    private Animator animator;
    private Vector2 movementInput;
    private IInteractable currentInteractableObject;
    private DirectionEnum currentInteractableRelativePosition;
    private AudioManager audioManager;

    public Vector2Int CurrentGridPosition => currentGridPosition;
    public DirectionEnum HoldInteractMoveDirection;


    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        gridManager = GridManager.Instance;
        currentGridPosition = gridManager.ConvertWorldPositionToGridPosition(transform.position);
        transform.position = gridManager.ConvertGridPositionToWorldPosition(currentGridPosition.x, currentGridPosition.y);
        targetWorldPosition = transform.position;
        InputManager.Instance.OnInteractPressed += HandleInteractionInput;
        InputManager.Instance.OnInteractStarted += SetInteractObject;
        audioManager = AudioManager.Instance;
    }

    void Update()
    {
        movementInput = InputManager.Instance.Movement;
        isPressingMove = InputManager.Instance.Movement != Vector2.zero;
        isHoldingInteract = InputManager.Instance.InteractHeld;

        
        HandleMovementInput();
        SmoothMoveToTarget();
        AnimationHandler();
        HandleSounds();
    }

    void HandleMovementInput()
    {
        stepCooldownTimer -= Time.deltaTime;
        if (stepCooldownTimer > 0f) return;

        DirectionEnum? moveDirection = GetDirectionFromInput();
        if (moveDirection == null) return;

        if (isHoldingInteract)
        {
            HandleInteractionInput();
        }
        else
        {
            TryMove(moveDirection.Value);
        }
    }

    void SetInteractObject() 
    {
        var currentFacingDirection = lastMoveDirection;//GetDirectionFromInput();

        currentInteractableRelativePosition = (DirectionEnum)currentFacingDirection;
        Vector2Int interactObjectLocation = currentGridPosition + currentInteractableRelativePosition.ToVector();
        //Debug.Log($"Current Interactable Object location: ({interactObjectLocation.x}, {interactObjectLocation.y})");

        currentInteractableObject = gridManager.GetInteractableAtGridPosition(interactObjectLocation.x, interactObjectLocation.y);
        //Debug.Log($"Setting Current Interactable Object to {currentInteractableObject.GetType()}");
    }
    
    void HandleInteractionInput()
    {
        //Debug.Log("Interact button pressed, attempting interaction...");
        TryInteract();
    }

    public void TryInteract()
    {
        if (currentInteractableObject != null && currentInteractableObject.GetType() == typeof(PushableRock))
                {
            //Debug.Log("Rock is Pushable");
            if (isPressingMove) 
            {
                DirectionEnum? movementDirection = GetDirectionFromInput();
                // Attempt to Push Rock
                if (movementDirection == currentInteractableRelativePosition)
                {
                    // Rock tries to move first
                    if (currentInteractableObject.TryInteract(this))
                    {
                        TryMove((currentInteractableRelativePosition));
                    }
                }
                // Attempt to Pull Rock
                else if (movementDirection == currentInteractableRelativePosition.Opposite())
                {
                    // Player tries to move first
                    if (TryMove(currentInteractableRelativePosition.Opposite()))
                    {
                        currentInteractableObject.TryInteract(this);
                    }
                }
            }
        }
        // Interactables that aren't moving rocks
        else { currentInteractableObject?.TryInteract(this); }
    }




    bool TryMove(DirectionEnum moveDirection)
    {
        Vector2Int targetGridPosition = currentGridPosition + moveDirection.ToVector();
        lastMoveDirection = moveDirection;

        // Player cannot move to destination
        if (!gridManager.IsCellPassableByPlayer(targetGridPosition.x, targetGridPosition.y)) return false;

        // Player moves to destination
        gridManager.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, false);
        currentGridPosition = targetGridPosition;
        targetWorldPosition = gridManager.ConvertGridPositionToWorldPosition(
            currentGridPosition.x,
            currentGridPosition.y
        );

        gridManager.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, true);

        stepCooldownTimer = stepDelay;
        isWalking = true;
        return true;
    }

    void SmoothMoveToTarget()
    {
        if (!isWalking) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWorldPosition,
            movementSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetWorldPosition) < 0.001f)
        {
            transform.position = targetWorldPosition;
            isWalking = false;
        }
    }


    DirectionEnum? GetDirectionFromInput()
    {
        Vector2 movementInput = InputManager.Instance.Movement;

        if (movementInput == Vector2.zero) return null;

        if (Mathf.Abs(movementInput.x) >= Mathf.Abs(movementInput.y))
        {
            return movementInput.x > 0f ? DirectionEnum.East : DirectionEnum.West;
        }
        else
        {
            return movementInput.y > 0f ? DirectionEnum.North : DirectionEnum.South;
        }
    }

    void AnimationHandler()
    {
        animator.SetBool("IsWalking", isWalking);
        animator.SetFloat("MoveX", movementInput.x);
        animator.SetFloat("MoveY", movementInput.y);
    }

    void HandleSounds()
    {
        if (isWalking) 
        {
            GroundTileTypeEnum groundTileType = gridManager.GetSerializedTileAt(currentGridPosition.x, currentGridPosition.y).GroundTileType;
            audioManager.PlayFootstepSound(groundTileType);
        }
    }
}





