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

    // Tracks which axes each cell is powered on (for beam visuals on Omni tiles)
    private Dictionary<Vector2Int, (bool horizontal, bool vertical)> currentPoweredAxes
        = new Dictionary<Vector2Int, (bool, bool)>();

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
        var poweredAxes = new Dictionary<Vector2Int, (bool horizontal, bool vertical)>();

        // Track visited (cell, entryDirection) pairs to prevent infinite loops
        // while still allowing the same cell to be powered from multiple directions
        HashSet<(Vector2Int, DirectionEnum)> visited = new HashSet<(Vector2Int, DirectionEnum)>();

        // Queue now carries: (cell, the direction power ENTERED this cell from)
        // null = source tile (can output in all directions)
        Queue<(Vector2Int cell, DirectionEnum? entryDirection)> propagationQueue
            = new Queue<(Vector2Int, DirectionEnum?)>();

        // Seed from all RuneSource tiles
        for (int column = 0; column < gridManager.NumberOfColumns; column++)
        {
            for (int row = 0; row < gridManager.NumberOfRows; row++)
            {
                GroundTileData tile = gridManager.GetTileAt(column, row);
                if (tile == null || tile.GroundTileType != GroundTileTypeEnum.RuneSource) continue;

                Vector2Int sourcePosition = new Vector2Int(column, row);
                newlyPoweredCells.Add(sourcePosition);
                poweredAxes[sourcePosition] = (true, true); // sources power both axes
                propagationQueue.Enqueue((sourcePosition, null));
            }
        }

        // BFS through passable rune tiles
        while (propagationQueue.Count > 0)
        {
            var (currentCell, entryDir) = propagationQueue.Dequeue();
            GroundTileData currentTile = gridManager.GetTileAt(currentCell.x, currentCell.y);

            if (!CanTravelThroughTile(currentTile, currentCell)) continue;

            for (int directionIndex = 0; directionIndex < 4; directionIndex++)
            {
                DirectionEnum travelDirection = (DirectionEnum)directionIndex;

                // For Omni tiles: only allow output opposite to how power entered
                if (!CanOutputInDirection(currentCell, travelDirection, entryDir)) continue;

                // Check current cell can connect in this direction (channel/block check)
                if (!CellCanConnectInDirection(currentCell, travelDirection)) continue;

                Vector2Int neighbourCell = currentCell + travelDirection.ToVector();
                DirectionEnum neighbourEntryDir = Connections.Opp(travelDirection);

                // Skip if we already processed this cell from this exact entry direction
                if (visited.Contains((neighbourCell, neighbourEntryDir))) continue;

                GroundTileData neighbourTile = gridManager.GetTileAt(neighbourCell.x, neighbourCell.y);
                if (neighbourTile == null) continue;
                if (!IsRunePassable(neighbourTile, neighbourCell)) continue;

                // Check neighbour can receive from the opposite direction
                if (!CellCanConnectInDirection(neighbourCell, neighbourEntryDir)) continue;

                visited.Add((neighbourCell, neighbourEntryDir));
                newlyPoweredCells.Add(neighbourCell);

                // Track which axes this cell is powered on (for beam visuals)
                bool isHorizontal = neighbourEntryDir == DirectionEnum.East || neighbourEntryDir == DirectionEnum.West;
                bool isVertical = neighbourEntryDir == DirectionEnum.North || neighbourEntryDir == DirectionEnum.South;

                if (poweredAxes.TryGetValue(neighbourCell, out var existing))
                {
                    poweredAxes[neighbourCell] = (
                        existing.horizontal || isHorizontal,
                        existing.vertical || isVertical
                    );
                }
                else
                {
                    poweredAxes[neighbourCell] = (isHorizontal, isVertical);
                }

                propagationQueue.Enqueue((neighbourCell, neighbourEntryDir));
            }
        }

        currentPoweredAxes = poweredAxes;
        ApplyPowerState(newlyPoweredCells);
        previouslyPoweredCells = newlyPoweredCells;

        CheckAllReceiversPowered();
    }

    // ── Tile checks ──

    /// <summary>
    /// For Omni tiles: power only passes straight through (left→right, up→down).
    /// All other tile types are unrestricted here (their directionality is handled
    /// by CellCanConnectInDirection).
    /// </summary>
    bool CanOutputInDirection(Vector2Int cell, DirectionEnum outputDir, DirectionEnum? entryDir)
    {
        // Sources (no entry direction) can output in all directions
        if (entryDir == null) return true;

        // Rotatable blocks handle their own directionality via ActiveConnections
        if (gridManager.GetRotatableRuneBlockAt(cell.x, cell.y) != null) return true;

        GroundTileData tile = gridManager.GetTileAt(cell.x, cell.y);
        if (tile == null) return true;

        // Omni = passthrough only: output must be opposite of entry
        if (tile.RuneChannel == RuneChannelTypeEnum.Omni)
        {
            return outputDir == Connections.Opp(entryDir.Value);
        }

        // All other types: no additional restriction
        return true;
    }

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
                // Sources, receivers, and omni can physically connect in all directions.
                // (Omni output restriction is handled by CanOutputInDirection)
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
        foreach (Vector2Int cell in newlyPoweredCells)
        {
            gridManager.GetRotatableRuneBlockAt(cell.x, cell.y)?.SetPowered(true);
            gridManager.GetReceiverAt(cell.x, cell.y)?.SetPowered(true);
            UpdateBeamVisual(cell, true);
        }

        foreach (Vector2Int cell in previouslyPoweredCells)
        {
            if (newlyPoweredCells.Contains(cell)) continue;
            gridManager.GetRotatableRuneBlockAt(cell.x, cell.y)?.SetPowered(false);
            gridManager.GetReceiverAt(cell.x, cell.y)?.SetPowered(false);
            UpdateBeamVisual(cell, false);
        }


    }


    void UpdateBeamVisual(Vector2Int cell, bool powered)
    {
        RuneBeamVisual beam = gridManager.GetBeamVisualAt(cell.x, cell.y);
        if (beam == null) return;

        if (!powered)
        {
            beam.SetPowered(false, false);
            return;
        }

        // Use tracked axes so Omni tiles only show beams for the directions
        // power actually passes through, not all 4
        if (currentPoweredAxes.TryGetValue(cell, out var axes))
        {
            beam.SetPowered(axes.horizontal, axes.vertical);
        }
        else
        {
            beam.SetPowered(false, false);
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