using UnityEngine;
using System.Collections.Generic;

public class DoorBehaviour : MonoBehaviour
{

    private GridManager gridManager;
    private Vector2Int gridPosition;
    private bool isDoorOpen = false;

    [SerializeField] GameObject visual;

    public bool IsDoorOpen => isDoorOpen;

    public Vector2Int GridPosition => gridPosition;

    void Start()
    {
        gridManager = GridManager.Instance;
        gridPosition = gridManager.ConvertWorldPositionToGridPosition(transform.position);
        gridManager.SetDoorState(gridPosition.x, gridPosition.y, isDoorOpen);

        //get the GameEventListener attached to this door and subscribe to its event
    }

    void SetDoorOpen(bool isOpen)
    {
        isDoorOpen = isOpen;
        gridManager.SetDoorState(gridPosition.x, gridPosition.y, isOpen);
        visual.SetActive(!isOpen);
        Debug.Log($"{gameObject.name} {(isOpen ? "opened" : "closed")}");
    }
}