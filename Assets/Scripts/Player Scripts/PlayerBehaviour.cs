using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] private float stepDelay = 0.15f;
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private int footstepsPerMovementBetweenTiles = 2;

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

    private float travelTimePerTile;




    void Start()
    {
        animator = GetComponent<Animator>();
        gridManager = GridManager.Instance;
        //transform.position = SceneChangeData.Instance.playerStartLocation;
 
        if(SceneChangeData.Instance != null)
        {
               transform.position = gridManager.ConvertGridPositionToWorldPosition(SceneChangeData.Instance.playerStartLocation.x, SceneChangeData.Instance.playerStartLocation.y);
        }
        else
        {
            Debug.LogWarning("SceneChangeData instance is null. Defaulting player start location to (0,0).");
        }
        currentGridPosition = gridManager.ConvertWorldPositionToGridPosition(transform.position);
        targetWorldPosition = transform.position;
        InputManager.Instance.OnInteractPressed += HandleInteractionInput;
        InputManager.Instance.OnInteractStarted += SetInteractObject;
        // we use this to get how long it takes the player to move tile to tile for the sound manager since its consistant
        travelTimePerTile = GridManager.Instance.WorldTileSize / movementSpeed; 
    }

    void Update()
    {
        movementInput = InputManager.Instance.Movement;
        isPressingMove = InputManager.Instance.Movement != Vector2.zero;
        isHoldingInteract = InputManager.Instance.InteractHeld;

        if(SceneChangeData.Instance != null)
        {
            if (SceneChangeData.Instance.isPaused || SceneChangeData.Instance.isInDiagloue)
            {
                animator.SetFloat("Speed", 0f);
                return;
            }
        }

        HandleMovementInput();
        SmoothMoveToTarget();
        //AnimationHandler();

    }




    void HandleMovementInput()
    {
        stepCooldownTimer -= Time.deltaTime;
        if (stepCooldownTimer > 0f) return;
        if (isWalking) return;

        DirectionEnum? moveDirection = GetDirectionFromInput();

        if (moveDirection == null)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        if (isHoldingInteract)
            HandleInteractionInput();
        else
            TryMove(moveDirection.Value);
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

        if (!gridManager.IsCellPassableByPlayer(targetGridPosition.x, targetGridPosition.y)) return false;

        Vector2Int dir = lastMoveDirection.ToVector();
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
        animator.SetFloat("Speed", 1f);

        GroundTileTypeEnum liftTileType = gridManager.GetSerializedTileAt(currentGridPosition.x, currentGridPosition.y).GroundTileType;
        GroundTileTypeEnum landTileType = gridManager.GetSerializedTileAt(targetGridPosition.x, targetGridPosition.y).GroundTileType;
        StartCoroutine(PlayTwoFootsteps(liftTileType, landTileType));

        gridManager.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, false);
        currentGridPosition = targetGridPosition;
        targetWorldPosition = gridManager.ConvertGridPositionToWorldPosition(currentGridPosition.x, currentGridPosition.y);
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

            if (!isPressingMove)
                animator.SetFloat("Speed", 0f);
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

    //void AnimationHandler()
    //{

    //    Vector2Int directionVector = lastMoveDirection.ToVector();
    //    animator.SetFloat("MoveX", directionVector.x);
    //    animator.SetFloat("MoveY", directionVector.y);
    //}

    //void HandleSounds()
    //{
    //    if (isWalking) 
    //    {
    //        GroundTileTypeEnum groundTileType = gridManager.GetSerializedTileAt(currentGridPosition.x, currentGridPosition.y).GroundTileType;
    //        audioManager.PlayFootstepSound(groundTileType);
    //    }
    //}

    public void SetPlayerLocation(Vector2Int newPlayerCoordinates)
    {
        currentGridPosition = newPlayerCoordinates;

        //// Player moves to destination
        //gridManager.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, false);
        //currentGridPosition = newPlayerCoordinates;
        //targetWorldPosition = gridManager.ConvertGridPositionToWorldPosition(
        //    currentGridPosition.x,
        //    currentGridPosition.y
        //);

        //gridManager.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, true);

        ////stepCooldownTimer = stepDelay;
        //isWalking = false;
        ////transform.position = targetWorldPosition;
    }

    public void SetPlayerFaceDirection(DirectionEnum directionFacing)
    {
        lastMoveDirection = directionFacing;
    }

    private IEnumerator PlayTwoFootsteps(GroundTileTypeEnum startTileType, GroundTileTypeEnum destinationTileType)
    {
        int liftSteps = footstepsPerMovementBetweenTiles / 2;
        int landSteps = footstepsPerMovementBetweenTiles - liftSteps; // odd remainder goes to destination, eg, 3 steps, 1 on start, 2 on destination

        float interval = travelTimePerTile / footstepsPerMovementBetweenTiles;

        for (int i = 0; i < liftSteps; i++)
        {
            AudioManager.Instance.PlayFootstepSound(startTileType);
            yield return new WaitForSeconds(interval);
        }

        for (int i = 0; i < landSteps; i++)
        {
            AudioManager.Instance.PlayFootstepSound(destinationTileType);
            yield return new WaitForSeconds(interval);
        }
    }
}





