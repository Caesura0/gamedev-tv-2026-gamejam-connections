using UnityEngine;
using System.Collections.Generic;

public class RunePowerSystem : MonoBehaviour
{
    public static RunePowerSystem Instance { get; private set; }

    // Fired when a tile's rune power state changes: (column, row, isPowered)
    public event System.Action<int, int, bool> OnTileRunePowerChanged;

    // Fired when all RuneReceivers are powered
    public event System.Action OnAllReceiversPowered;

    private GridManager gridManager;
    private HashSet<Vector2Int> previouslyPoweredCells = new HashSet<Vector2Int>();

    void Awake()
    {
        Instance = this;

        if (GridManager.Instance != null)
            GridManager.Instance.SubscribeToRuneEvents();
    }

    void Start()
    {
        gridManager = GridManager.Instance;
        RunEnergyThrough();
    }

    // ── Propagation ──

    public void RunEnergyThrough()
    {
        HashSet<Vector2Int> newlyPoweredCells = new HashSet<Vector2Int>();
        Queue<Vector2Int> propagationQueue = new Queue<Vector2Int>();

        // Seed from all RuneSource tiles
        for (int column = 0; column < gridManager.NumberOfColumns; column++)
        {
            for (int row = 0; row < gridManager.NumberOfRows; row++)
            {
                GroundTileData tile = gridManager.GetTileAt(column, row);
                if (tile == null || tile.GroundTileType != GroundTileTypeEnum.RuneSource) continue;

                Vector2Int sourcePosition = new Vector2Int(column, row);
                newlyPoweredCells.Add(sourcePosition);
                propagationQueue.Enqueue(sourcePosition);
            }
        }

        // BFS through passable rune tiles
        while (propagationQueue.Count > 0)
        {
            Vector2Int currentCell = propagationQueue.Dequeue();
            GroundTileData currentTile = gridManager.GetTileAt(currentCell.x, currentCell.y);

            if (!CanTravelThroughTile(currentTile, currentCell)) continue;

            for (int directionIndex = 0; directionIndex < 4; directionIndex++)
            {
                DirectionEnum travelDirection = (DirectionEnum)directionIndex;
                Vector2Int neighbourCell = currentCell + travelDirection.ToVector();

                if (newlyPoweredCells.Contains(neighbourCell)) continue;

                // Check current cell can output in this direction
                if (!CellCanConnectInDirection(currentCell, travelDirection)) continue;

                GroundTileData neighbourTile = gridManager.GetTileAt(neighbourCell.x, neighbourCell.y);
                if (neighbourTile == null) continue;
                if (!IsRunePassable(neighbourTile, neighbourCell)) continue;

                // Check neighbour can receive from the opposite direction
                if (!CellCanConnectInDirection(neighbourCell, Connections.Opp(travelDirection))) continue;

                newlyPoweredCells.Add(neighbourCell);
                propagationQueue.Enqueue(neighbourCell);
            }
        }

        ApplyPowerState(newlyPoweredCells);
        previouslyPoweredCells = newlyPoweredCells;

        CheckAllReceiversPowered();
    }

    // ── Tile checks ──

    bool CanTravelThroughTile(GroundTileData tile, Vector2Int cell)
    {
        if (tile == null) return false;
        if (gridManager.GetRotatableRuneBlockAt(cell.x, cell.y) != null) return true;

        return tile.GroundTileType == GroundTileTypeEnum.RuneSource ||
               tile.GroundTileType == GroundTileTypeEnum.RuneReceiver ||
               tile.RuneChannel != RuneChannelTypeEnum.None;
    }

    bool IsRunePassable(GroundTileData tile, Vector2Int cell)
    {
        if (tile == null) return false;
        if (gridManager.GetRotatableRuneBlockAt(cell.x, cell.y) != null) return true;

        return tile.GroundTileType == GroundTileTypeEnum.RuneReceiver ||
               tile.RuneChannel != RuneChannelTypeEnum.None;
    }

    // Returns true if the cell can connect in the given direction.
    // Horizontal/Vertical channels are axis-locked; Omni and rotatable blocks
    // handle direction via their own rules.
    bool CellCanConnectInDirection(Vector2Int cell, DirectionEnum direction)
    {
        RotatableRuneBlock block = gridManager.GetRotatableRuneBlockAt(cell.x, cell.y);
        if (block != null)
            return block.ActiveConnections[(int)direction];

        GroundTileData tile = gridManager.GetTileAt(cell.x, cell.y);
        if (tile == null) return false;

        switch (tile.RuneChannel)
        {
            case RuneChannelTypeEnum.Horizontal:
                return direction == DirectionEnum.East || direction == DirectionEnum.West;

            case RuneChannelTypeEnum.Vertical:
                return direction == DirectionEnum.North || direction == DirectionEnum.South;

            case RuneChannelTypeEnum.Omni:
            case RuneChannelTypeEnum.None:
            default:
                // Sources and receivers connect in all directions
                return true;
        }
    }

    // ── Apply state ──

    void ApplyPowerState(HashSet<Vector2Int> newlyPoweredCells)
    {
        // Tiles that gained power
        foreach (Vector2Int cell in newlyPoweredCells)
        {
            GroundTileData tile = gridManager.GetTileAt(cell.x, cell.y);
            if (tile == null || tile.IsRunePowered) continue;

            tile.IsRunePowered = true;
            OnTileRunePowerChanged?.Invoke(cell.x, cell.y, true);
        }

        // Tiles that lost power
        foreach (Vector2Int cell in previouslyPoweredCells)
        {
            if (newlyPoweredCells.Contains(cell)) continue;

            GroundTileData tile = gridManager.GetTileAt(cell.x, cell.y);
            if (tile == null || !tile.IsRunePowered) continue;

            tile.IsRunePowered = false;
            OnTileRunePowerChanged?.Invoke(cell.x, cell.y, false);
        }
    }

    void CheckAllReceiversPowered()
    {
        for (int column = 0; column < gridManager.NumberOfColumns; column++)
        {
            for (int row = 0; row < gridManager.NumberOfRows; row++)
            {
                GroundTileData tile = gridManager.GetTileAt(column, row);
                if (tile == null || tile.GroundTileType != GroundTileTypeEnum.RuneReceiver) continue;
                if (!tile.IsRunePowered) return;
            }
        }

        OnAllReceiversPowered?.Invoke();
    }
}