using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public int NumberOfColumns = 10;
    public int NumberOfRows = 8;
    public float WorldTileSize = 1f;

    // Serialized so the tile data is saved with the scene and visible to the custom editor
    [SerializeField] private GroundTileData[] serializedTileGridArray;

    private GroundTileData[,] tileGrid;

    // Fired when a pressure plate is activated or deactivated: (column, row, isActivated)
    public event System.Action<int, int, bool> OnPressurePlateStateChanged;

    void Awake()
    {
        Instance = this;
        InitialiseGrid();
    }


    void InitialiseGrid()
    {
        tileGrid = new GroundTileData[NumberOfColumns, NumberOfRows];

        for (int column = 0; column < NumberOfColumns; column++)
            for (int row = 0; row < NumberOfRows; row++)
                tileGrid[column, row] = GetSerializedTileAt(column, row);
    }


    // Reads from the serializedTileGrid, falls back to Hedge if not yet painted
    GroundTileData GetSerializedTileAt(int column, int row)
    {
        int index = row * NumberOfColumns + column;

        if (serializedTileGridArray != null && index < serializedTileGridArray.Length)
            return serializedTileGridArray[index];

        return new GroundTileData { GroundTileType = GroundTileTypeEnum.Hedge };
    }


    // Called by the editor script when the grid size changes or a cell is painted
    public void RebuildSerializedGrid()
    {
        int totalCells = NumberOfColumns * NumberOfRows;
        var previousGrid = serializedTileGridArray;
        serializedTileGridArray = new GroundTileData[totalCells];

        for (int column = 0; column < NumberOfColumns; column++)
        {
            for (int row = 0; row < NumberOfRows; row++)
            {
                int index = row * NumberOfColumns + column;

                // Preserve any existing painted data
                if (previousGrid != null && index < previousGrid.Length)
                    serializedTileGridArray[index] = previousGrid[index];
                else
                    serializedTileGridArray[index] = new GroundTileData { GroundTileType = GroundTileTypeEnum.Hedge };
            }
        }
    }


    // Called by the editor script when a cell is painted
    public void PaintTile(int column, int row, GroundTileTypeEnum groundTileType)
    {
        if (!IsCellInBounds(column, row)) return;

        int index = row * NumberOfColumns + column;

        if (serializedTileGridArray == null || index >= serializedTileGridArray.Length)
            RebuildSerializedGrid();

        serializedTileGridArray[index].GroundTileType = groundTileType;
    }


    // Runtime cell changes - currently only used for pushing rocks onto water, but could be expanded for other gameplay interactions

    public void SetCellMoveableOccupancy(int column, int row, bool isOccupied)
    {
        if (!IsCellInBounds(column, row)) return;

        GroundTileData tile = tileGrid[column, row];

        // Rock pushed onto water — convert to stone and mark occupied
        if (isOccupied && tile.GroundTileType == GroundTileTypeEnum.Water)
        {
            tile.GroundTileType = GroundTileTypeEnum.Stone;
            // Also update the serialized grid so the change is visible in the editor
            int index = row * NumberOfColumns + column;
            if (serializedTileGridArray != null && index < serializedTileGridArray.Length)
                serializedTileGridArray[index].GroundTileType = GroundTileTypeEnum.Stone;
        }

        tile.IsOccupiedByMoveable = isOccupied;

        // Pressure plate activation
        if (tile.GroundTileType == GroundTileTypeEnum.PressurePlate)
        {
            bool wasActivated = tile.IsPressurePlateActivated;
            tile.IsPressurePlateActivated = isOccupied;

            if (tile.IsPressurePlateActivated != wasActivated)
                OnPressurePlateStateChanged?.Invoke(column, row, tile.IsPressurePlateActivated);
        }
    }


    public GroundTileData GetTileAt(int column, int row)
    {
        if (!IsCellInBounds(column, row)) return null;
        return tileGrid[column, row];
    }




    public bool IsCellInBounds(int column, int row)
    {
        return column >= 0 && row >= 0 && column < NumberOfColumns && row < NumberOfRows;
    }




    public bool IsCellPassableByPlayer(int column, int row)
    {
        GroundTileData tile = GetTileAt(column, row);
        return tile != null && tile.IsPassableByPlayer && !tile.IsOccupiedByMoveable;
    }



    public bool IsCellValidRockDestination(int column, int row)
    {
        GroundTileData tile = GetTileAt(column, row);
        return tile != null && tile.IsValidRockDestination;
    }




    // Coordinate conversions, assumes grid origin is at the GameObject's position and grid is aligned with world axes

    public Vector2Int ConvertWorldPositionToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - transform.position;
        return new Vector2Int(
            Mathf.FloorToInt(localPosition.x / WorldTileSize),
            Mathf.FloorToInt(localPosition.y / WorldTileSize)
        );
    }



    public Vector3 ConvertGridPositionToWorldPosition(int column, int row)
    {
        return 
            transform.position + new Vector3(
                column * WorldTileSize + WorldTileSize * 0.5f,
                row * WorldTileSize + WorldTileSize * 0.5f,
                0f);

    }




#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        DrawPaintedTiles();
        DrawCellGrid();
        DrawGridBounds();
        DrawOriginMarker();
    }

    void DrawPaintedTiles()
    {
        if (serializedTileGridArray == null) return;

        for (int column = 0; column < NumberOfColumns; column++)
        {
            for (int row = 0; row < NumberOfRows; row++)
            {
                int index = row * NumberOfColumns + column;
                if (index >= serializedTileGridArray.Length) continue;

                GroundTileData tile = serializedTileGridArray[index];

                Gizmos.color = GetTileGizmoColor(tile.GroundTileType);

                Vector3 center = transform.position + new Vector3(
                    column * WorldTileSize + WorldTileSize * 0.5f,
                    row * WorldTileSize + WorldTileSize * 0.5f,
                    0f
                );

                float insetSize = WorldTileSize * 0.9f;
                Gizmos.DrawCube(center, new Vector3(insetSize, insetSize, 0f));
            }
        }
    }

    Color GetTileGizmoColor(GroundTileTypeEnum tileType)
    {
        Color color = TileEditorColors.GetColorForTileType(tileType);
        color.a = 0.85f;
        return color;
    }

    void DrawCellGrid()
    {
        Vector3 origin = transform.position;
        float totalWidth = NumberOfColumns * WorldTileSize;
        float totalHeight = NumberOfRows * WorldTileSize;

        Gizmos.color = new Color(1f, 1f, 1f, 0.15f);

        for (int column = 1; column < NumberOfColumns; column++)
        {
            float x = column * WorldTileSize;
            Gizmos.DrawLine(origin + new Vector3(x, 0, 0), origin + new Vector3(x, totalHeight, 0));
        }

        for (int row = 1; row < NumberOfRows; row++)
        {
            float y = row * WorldTileSize;
            Gizmos.DrawLine(origin + new Vector3(0, y, 0), origin + new Vector3(totalWidth, y, 0));
        }
    }

    void DrawGridBounds()
    {
        float totalWidth = NumberOfColumns * WorldTileSize;
        float totalHeight = NumberOfRows * WorldTileSize;
        Vector3 origin = transform.position;

        Gizmos.color = new Color(1f, 1f, 1f, 0.7f);
        Gizmos.DrawLine(origin, origin + new Vector3(totalWidth, 0, 0));
        Gizmos.DrawLine(origin + new Vector3(totalWidth, 0, 0), origin + new Vector3(totalWidth, totalHeight, 0));
        Gizmos.DrawLine(origin + new Vector3(totalWidth, totalHeight, 0), origin + new Vector3(0, totalHeight, 0));
        Gizmos.DrawLine(origin + new Vector3(0, totalHeight, 0), origin);
    }

    void DrawOriginMarker()
    {
        float crossSize = WorldTileSize * 0.2f;
        Vector3 origin = transform.position;

        Gizmos.color = new Color(1f, 0.8f, 0f, 0.9f);
        Gizmos.DrawLine(origin + Vector3.left * crossSize, origin + Vector3.right * crossSize);
        Gizmos.DrawLine(origin + Vector3.down * crossSize, origin + Vector3.up * crossSize);

        UnityEditor.Handles.color = new Color(1f, 0.8f, 0f, 0.8f);
        UnityEditor.Handles.Label(
            origin + new Vector3(0.05f, -0.35f, 0),
            $"{NumberOfColumns} x {NumberOfRows}  (tile: {WorldTileSize})"
        );
    }
#endif
}