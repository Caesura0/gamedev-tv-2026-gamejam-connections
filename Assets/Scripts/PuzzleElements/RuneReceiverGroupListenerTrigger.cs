using UnityEngine;
using System.Collections.Generic;

public class RuneReceiverGroupListenerTrigger : MonoBehaviour
{
    [SerializeField] private List<Vector2Int> requiredReceiverPositions = new List<Vector2Int>();
    [SerializeField] private Sprite inactiveReceiverSprite;
    [SerializeField] private Sprite activeReceiverSprite;   
    
    public event System.Action<bool> OnGroupStateChanged;

    private GridManager gridManager;
    private bool isGroupActive = false;

    public bool IsGroupActive => isGroupActive;

    void Start()
    {
        gridManager = GridManager.Instance;
        RunePowerSystem.Instance.OnTileRunePowerChanged += HandleRunePowerChanged;
        Evaluate();
    }

    void OnDestroy()
    {
        if (RunePowerSystem.Instance != null)
            RunePowerSystem.Instance.OnTileRunePowerChanged -= HandleRunePowerChanged;
    }

    void HandleRunePowerChanged(int column, int row, bool isPowered)
    {
        Evaluate();
    }

    void Evaluate()
    {
        bool allActive = AreAllReceiversPowered();
        if (allActive == isGroupActive) return;

        isGroupActive = allActive;
        OnGroupStateChanged?.Invoke(isGroupActive);
    }

    bool AreAllReceiversPowered()
    {
        if (requiredReceiverPositions.Count == 0) return false;

        foreach (Vector2Int position in requiredReceiverPositions)
        {
            GroundTileData tile = gridManager.GetTileAt(position.x, position.y);
            if (tile == null || tile.GroundTileType != GroundTileTypeEnum.RuneReceiver) return false;
            if (!tile.IsRunePowered) return false;
        }

        return true;
    }
}