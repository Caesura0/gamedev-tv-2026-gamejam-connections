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
    }

    void Update()
    {
        movementInput = InputManager.Instance.Movement;
        isPressingMove = InputManager.Instance.Movement != Vector2.zero;
        isHoldingInteract = InputManager.Instance.InteractHeld;

        HandleMovementInput();
        SmoothMoveToTarget();
        AnimationHandler();
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

        //InteractObjectLocation = currentGridPosition + Vector2Int.RoundToInt(movementInput);
        //Vector2Int facingCell = currentGridPosition + lastMoveDirection.ToVector();
        //// FIXME for null references - interact pushed when no object
        //// FIXME for getting correct rock
        //IInteractable interactable = gridManager.GetInteractableAtGridPosition(facingCell.x, facingCell.y);

        //bool isPushableRock = interactable.GetType() == typeof(PushableRock);

        // Trying to work out code to push/pull
        
        if (currentInteractableObject != null && currentInteractableObject.GetType() == typeof(PushableRock))
        //if (interactable != null && isPushableRock)
        {
            //Debug.Log("Rock is Pushable");
            if (isPressingMove) 
            {
                DirectionEnum? movementDirection = GetDirectionFromInput();
                //Debug.Log("Is trying to push rock");
                if (movementDirection == currentInteractableRelativePosition)
                {
                    if (currentInteractableObject.TryInteract(this))
                    {
                        TryMove((currentInteractableRelativePosition));
                    }
                }
                else if (movementDirection == currentInteractableRelativePosition.Opposite())
                {
                    
                    if (TryMove(currentInteractableRelativePosition.Opposite()))
                    {
                        currentInteractableObject.TryInteract(this);
                    }
                }
                //if (validMoveDirection)
                //{ 
                //    Debug.Log("Push direction valid");
                //    if (interactable.TryInteract(this))
                //    {
                //        TryMove(lastMoveDirection);
                //    }
                //}
            }
            //Debug.Log("Trying to push rock in valid direction");
            // Check if move button also pressed

            // Check if valid direction

            // Rock can be pushed in given direction
            // FIXME: Change direction player and rock move to the current held arrow key while interact held
            // Player and rock can move into each others space as long as other can move...
            
            // Rock cannot be pushed
            else { }
        }
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

}





