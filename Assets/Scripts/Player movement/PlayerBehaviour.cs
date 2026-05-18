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

    private Animator animator;
    private Vector2 movementInput;

    public Vector2Int CurrentGridPosition => currentGridPosition;


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

        TryMove(moveDirection.Value);
    }

    void HandleInteractionInput()
    {
        Debug.Log("Interact button pressed, attempting interaction...");
        TryInteract();
        
    }

    public void TryInteract()
    {
        Vector2Int facingCell = currentGridPosition + lastMoveDirection.ToVector();
        IInteractable interactable = gridManager.GetInteractableAtGridPosition(facingCell.x, facingCell.y);

        interactable?.TryInteract(this);
    }




    void TryMove(DirectionEnum moveDirection)
    {
        Vector2Int targetGridPosition = currentGridPosition + moveDirection.ToVector();
        lastMoveDirection = moveDirection;

        if (!gridManager.IsCellPassableByPlayer(targetGridPosition.x, targetGridPosition.y)) return;

        gridManager.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, false);
        currentGridPosition = targetGridPosition;
        targetWorldPosition = gridManager.ConvertGridPositionToWorldPosition(
            currentGridPosition.x,
            currentGridPosition.y
        );

        gridManager.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, true);

        stepCooldownTimer = stepDelay;
        isWalking = true;
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





