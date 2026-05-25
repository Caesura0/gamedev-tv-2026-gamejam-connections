using UnityEngine;

public class PushableRock : MonoBehaviour, IInteractable
{
    
    private Vector2Int currentGridPosition;

    private Vector3 targetWorldPosition;
    private bool isMoving;
    private float moveSpeed = 10f;

    void Start()
    {

        currentGridPosition = GridManager.Instance.ConvertWorldPositionToGridPosition(transform.position);

        GridManager.Instance.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, true);
        GridManager.Instance.RegisterInteractable(currentGridPosition.x, currentGridPosition.y, this);
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

        if (!GridManager.Instance.IsCellValidRockDestination(targetGridPosition.x, targetGridPosition.y))
            return false;

        GroundTileData tile = GridManager.Instance.GetTileAt(currentGridPosition.x, currentGridPosition.y);
        if (tile.IsInWater)
            return false;

        GridManager.Instance.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, false);
        GridManager.Instance.RegisterInteractable(currentGridPosition.x, currentGridPosition.y, null);

        currentGridPosition = targetGridPosition;
        targetWorldPosition = GridManager.Instance.ConvertGridPositionToWorldPosition(
            currentGridPosition.x,
            currentGridPosition.y
        );

        GridManager.Instance.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, true);
        GridManager.Instance.RegisterInteractable(currentGridPosition.x, currentGridPosition.y, this);

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