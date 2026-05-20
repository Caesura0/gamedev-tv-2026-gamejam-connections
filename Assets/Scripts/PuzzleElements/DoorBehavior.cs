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

    GameEventListener gameEventListener;

    bool isConditionsMet = false;


    void Start()
    {
        gridManager = GridManager.Instance;
        gridPosition = gridManager.ConvertWorldPositionToGridPosition(transform.position);
        gridManager.SetDoorState(gridPosition.x, gridPosition.y, isDoorOpen);

        if (TryGetComponent(out gameEventListener))
        {
            gameEventListener.OnFullConditionMet += GameEventListener_OnFullConditionMet; ;
        }
    }

    private void GameEventListener_OnFullConditionMet(bool obj)
    {
        isConditionsMet = obj;
        SetDoorOpen(isConditionsMet);
    }

    void SetDoorOpen(bool isOpen)
    {
        isDoorOpen = isOpen;
        gridManager.SetDoorState(gridPosition.x, gridPosition.y, isOpen);
        visual.SetActive(!isOpen);
        Debug.Log($"{gameObject.name} {(isOpen ? "opened" : "closed")}");
    }
}