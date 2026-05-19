using UnityEngine;

public class PushableRock : MonoBehaviour, IInteractable
{
    private GridManager gridManager;
    private Vector2Int currentGridPosition;

    void Start()
    {
        gridManager = GridManager.Instance;
        currentGridPosition = gridManager.ConvertWorldPositionToGridPosition(transform.position);

        gridManager.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, true);
        gridManager.RegisterInteractable(currentGridPosition.x, currentGridPosition.y, this);
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
        transform.position = gridManager.ConvertGridPositionToWorldPosition(
            currentGridPosition.x,
            currentGridPosition.y
        );

        gridManager.SetCellMoveableOccupancy(currentGridPosition.x, currentGridPosition.y, true);
        gridManager.RegisterInteractable(currentGridPosition.x, currentGridPosition.y, this);

        return true;
    }

    //public bool TryInteract(PlayerBehaviour player)
    //{
    //    // Calculate push direction from player position to rock position
    //    Vector2Int pushDirection = currentGridPosition - player.CurrentGridPosition;
    //    Debug.Log($"Attempting to push rock in direction {pushDirection}");
    //    return TryMove(pushDirection);
    //}
    public bool TryInteract(PlayerBehaviour player)
    {
        // Calculate push direction from player position to rock position
        Vector2Int pushDirection = Vector2Int.RoundToInt(InputManager.Instance.Movement);//currentGridPosition - player.CurrentGridPosition;
        Debug.Log($"Attempting to push rock in direction {pushDirection}");
        return TryMove(pushDirection);
    }

    public bool TryInteractAlternate(PlayerBehaviour player)
    {
        // Calculate pull direction from player position to rock position
        Vector2Int pullDirection = player.CurrentGridPosition - currentGridPosition;
        Debug.Log($"Attempting to pull rock in direction {pullDirection}");
        return TryMove(pullDirection);
        //Debug.Log("Alternate interaction with pushable rock - no effect");
        //return false;
    }
}