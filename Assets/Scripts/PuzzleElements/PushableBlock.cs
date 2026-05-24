using UnityEngine;

public class PushableRock : MonoBehaviour, IInteractable
{
    private GridManager gridManager;
    private Vector2Int currentGridPosition;

    private Vector3 targetWorldPosition;
    private bool isMoving;
    private float moveSpeed = 10f;
    void Start()
    {
        gridManager = GridManager.Instance;
        currentGridPosition = gridManager.ConvertWorldPositionToGridPosition(transform.position);

        gridManager.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, true);
        gridManager.RegisterInteractable(currentGridPosition.x, currentGridPosition.y, this);
    }


    void Update()
    {
        SmoothMoveToTarget();
    }

    void SmoothMoveToTarget()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWorldPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetWorldPosition) < 0.001f)
        {
            transform.position = targetWorldPosition;
            isMoving = false;
        }
    }



    bool TryMove(Vector2Int moveDirection)
    {
        Vector2Int targetGridPosition = currentGridPosition + moveDirection;

        if (!gridManager.IsCellValidRockDestination(targetGridPosition.x, targetGridPosition.y))
            return false;

        GroundTileData tile = gridManager.GetTileAt(currentGridPosition.x, currentGridPosition.y);
        if (tile.IsInWater)
            return false;

        gridManager.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, false);
        gridManager.RegisterInteractable(currentGridPosition.x, currentGridPosition.y, null);

        currentGridPosition = targetGridPosition;
        targetWorldPosition = gridManager.ConvertGridPositionToWorldPosition(
            currentGridPosition.x,
            currentGridPosition.y
        );

        gridManager.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, true);
        gridManager.RegisterInteractable(currentGridPosition.x, currentGridPosition.y, this);

        AudioManager.Instance.PlayRockSounds();
        isMoving = true;
        return true;
    }

    public bool TryInteract(PlayerBehaviour player)
    {
        Vector2Int pushDirection = Vector2Int.RoundToInt(InputManager.Instance.Movement);
        return TryMove(pushDirection);
    }

    public bool TryInteractAlternate(PlayerBehaviour player)
    {
        Vector2Int pullDirection = player.CurrentGridPosition - currentGridPosition;
        return TryMove(pullDirection);
    }
}