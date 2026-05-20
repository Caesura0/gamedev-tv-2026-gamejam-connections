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

    void HandleInteractionInput()
    {
        //Debug.Log("Interact button pressed, attempting interaction...");
        TryInteract();
    }

    public void TryInteract()
    {
        Vector2Int facingCell = currentGridPosition + lastMoveDirection.ToVector();
        // FIXME for null references - interact pushed when no object
        IInteractable interactable = gridManager.GetInteractableAtGridPosition(facingCell.x, facingCell.y);
        if (interactable == null) return;
        bool isPushableRock = interactable is PushableRock pushableRock;
        bool validMoveDirection = movementInput == lastMoveDirection.ToVector() || movementInput == -lastMoveDirection.ToVector();
        
        // Trying to work out code to push/pull
        if (interactable != null && isPushableRock)
        {
            //Debug.Log("Rock is Pushable");
            if (isPressingMove) 
            { 
                //Debug.Log("Is trying to push rock");
                if (movementInput == lastMoveDirection.ToVector())
                {
                    if (interactable.TryInteract(this))
                    {
                        TryMove((lastMoveDirection));
                    }
                }
                else if (movementInput == -lastMoveDirection.ToVector())
                {
                    
                    if (TryMove(lastMoveDirection.Opposite()))
                    {
                        interactable.TryInteract(this);
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
        else { interactable?.TryInteract(this); }
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





