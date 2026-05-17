using UnityEngine;
using System.Collections.Generic;

public class DoorBehaviour : MonoBehaviour
{
    [SerializeField] private List<Vector2Int> requiredPlatePositions = new List<Vector2Int>();

    private GridManager gridManager;
    private Vector2Int gridPosition;
    private bool isDoorOpen = false;

    [SerializeField] GameObject visual;

    public bool IsDoorOpen => isDoorOpen;

    public Vector2Int GridPosition => gridPosition;

    void Start()
    {
        gridManager = GridManager.Instance;
        gridManager.OnPressurePlateStateChanged += HandlePressurePlateStateChanged;
        gridPosition = GridManager.Instance.ConvertWorldPositionToGridPosition(transform.position);
        Debug.Log($"{gameObject.name} initialized at grid position {gridPosition}");
        gridManager.SetDoorState(gridPosition.x, gridPosition.y, isDoorOpen);

        EvaluateDoorState();
    }

    void OnDestroy()
    {
        if (gridManager != null)
            gridManager.OnPressurePlateStateChanged -= HandlePressurePlateStateChanged;
    }

    void HandlePressurePlateStateChanged(int column, int row, bool isActivated)
    {
        EvaluateDoorState();
    }

    void EvaluateDoorState()
    {
        bool allPlatesActive = AreAllRequiredPlatesActive();

        if (allPlatesActive == isDoorOpen) return;

        isDoorOpen = allPlatesActive;

        if (isDoorOpen)
            OpenDoor();
        else
            CloseDoor();
    }

    bool AreAllRequiredPlatesActive()
    {
        if (requiredPlatePositions.Count == 0) return false;

        foreach (Vector2Int platePosition in requiredPlatePositions)
        {
            GroundTileData tile = gridManager.GetTileAt(platePosition.x, platePosition.y);

            if (tile == null || !tile.IsPressurePlateActivated)
                return false;
        }

        return true;
    }

    void OpenDoor()
    {

        gridManager.SetDoorState(gridPosition.x, gridPosition.y, true);
        visual.SetActive(false);
        Debug.Log($"{gameObject.name} opened");
    }

    void CloseDoor()
    {

        gridManager.SetDoorState(gridPosition.x, gridPosition.y, false);
        visual.SetActive(true);

        Debug.Log($"{gameObject.name} closed");
    }
}