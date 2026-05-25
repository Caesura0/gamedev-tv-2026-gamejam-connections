using UnityEngine;
using System.Collections.Generic;

public class PressurePlateGroupListenerTrigger : MonoBehaviour
{
    [SerializeField] private List<Vector2Int> requiredPlatePositions = new List<Vector2Int>();

    public event System.Action<bool> OnGroupStateChanged;

 
    private bool isGroupActive = false;

    public bool IsGroupActive => isGroupActive;

    void Start()
    {

        GridManager.Instance.OnPressurePlateStateChanged += HandlePressurePlateStateChanged;
        Evaluate();
    }

    void OnDestroy()
    {
        if (GridManager.Instance != null)
            GridManager.Instance.OnPressurePlateStateChanged -= HandlePressurePlateStateChanged;
    }

    void HandlePressurePlateStateChanged(int column, int row, bool isActivated)
    {
        Evaluate();
    }

    void Evaluate()
    {
        bool allActive = AreAllPlatesActive();
        if (allActive == isGroupActive) return;

        isGroupActive = allActive;
        OnGroupStateChanged?.Invoke(isGroupActive);
    }

    bool AreAllPlatesActive()
    {
        if (requiredPlatePositions.Count == 0) return false;

        foreach (Vector2Int position in requiredPlatePositions)
        {
            GroundTileData tile = GridManager.Instance.GetTileAt(position.x, position.y);
            if (tile == null || !tile.IsPressurePlateActivated) return false;
        }

        return true;
    }
}