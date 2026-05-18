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
    }

    void Start()
    {
        gridManager = GridManager.Instance;
        RunEnergyThrough();
    }



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

            Debug.Log($"[POWER] Processing cell {currentCell}");

            if (!CanTravelThroughTile(currentTile, currentCell))
            {
                Debug.LogWarning($"[POWER] Cannot travel through {currentCell}");
                continue;
            }

            for (int directionIndex = 0; directionIndex < 4; directionIndex++)
            {
                DirectionEnum travelDirection = (DirectionEnum)directionIndex;
                Vector2Int neighbourCell = currentCell + travelDirection.ToVector();

                Debug.Log($"[POWER] Trying {travelDirection} from {currentCell} -> {neighbourCell}");

                if (newlyPoweredCells.Contains(neighbourCell))
                {
                    Debug.Log($"[POWER] {neighbourCell} already powered");
                    continue;
                }

                // Check current cell can output in this direction
                bool currentCanOutput = CellCanConnectInDirection(currentCell, travelDirection);

                Debug.Log(
                    $"[POWER] Current cell {currentCell} output {travelDirection}: {currentCanOutput}"
                );

                if (!currentCanOutput)
                {
                    Debug.LogWarning(
                        $"[POWER] BLOCKED: {currentCell} cannot output {travelDirection}"
                    );
                    continue;
                }

                GroundTileData neighbourTile = gridManager.GetTileAt(neighbourCell.x, neighbourCell.y);

                if (neighbourTile == null)
                {
                    Debug.LogWarning($"[POWER] No tile at {neighbourCell}");
                    continue;
                }

                bool neighbourPassable = IsRunePassable(neighbourTile, neighbourCell);

                Debug.Log(
                    $"[POWER] Neighbour {neighbourCell} passable: {neighbourPassable}"
                );

                if (!neighbourPassable)
                {
                    Debug.LogWarning(
                        $"[POWER] BLOCKED: neighbour {neighbourCell} is not rune passable"
                    );
                    continue;
                }

                DirectionEnum oppositeDirection = Connections.Opp(travelDirection);

                bool neighbourCanReceive =
                    CellCanConnectInDirection(neighbourCell, oppositeDirection);

                Debug.Log(
                    $"[POWER] Neighbour {neighbourCell} receive from {oppositeDirection}: {neighbourCanReceive}"
                );

                if (!neighbourCanReceive)
                {
                    Debug.LogWarning(
                        $"[POWER] BLOCKED: neighbour {neighbourCell} cannot receive from {oppositeDirection}"
                    );
                    continue;
                }

                Debug.Log(
                    $"[POWER] SUCCESS: Connected {currentCell} -> {neighbourCell}"
                );

                newlyPoweredCells.Add(neighbourCell);
                propagationQueue.Enqueue(neighbourCell);
            }
        }

        ApplyPowerState(newlyPoweredCells);
        previouslyPoweredCells = newlyPoweredCells;

        CheckAllReceiversPowered();
    }


    bool CanTravelThroughTile(GroundTileData tile, Vector2Int cell)
    {
        if (tile == null) return false;

        // Rotatable blocks are always travellable if they exist on this cell
        if (gridManager.GetRotatableRuneBlockAt(cell.x, cell.y) != null) return true;

        return tile.GroundTileType == GroundTileTypeEnum.RuneSource ||
               tile.GroundTileType == GroundTileTypeEnum.RuneChannel ||
               tile.GroundTileType == GroundTileTypeEnum.RuneReceiver;
    }

    bool IsRunePassable(GroundTileData tile, Vector2Int cell)
    {
        if (tile == null) return false;

        // Rotatable blocks can receive power on any passable tile
        if (gridManager.GetRotatableRuneBlockAt(cell.x, cell.y) != null) return true;

        return tile.GroundTileType == GroundTileTypeEnum.RuneChannel ||
               tile.GroundTileType == GroundTileTypeEnum.RuneReceiver;
    }

    // Returns true if the cell can connect in the given direction.
    // Fixed rune tiles connect in all directions; rotatable blocks use their connection table.
    bool CellCanConnectInDirection(Vector2Int cell, DirectionEnum direction)
    {
        RotatableRuneBlock block = gridManager.GetRotatableRuneBlockAt(cell.x, cell.y);

        if (block != null)
        {
            bool result = block.ActiveConnections[(int)direction];

            Debug.Log(
                $"[CONNECTION] Block at {cell} checking {direction} = {result}"
            );

            return result;
        }

        Debug.Log(
            $"[CONNECTION] Fixed tile at {cell} automatically connects {direction}"
        );

        // Fixed tiles connect in all directions
        return true;
    }

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