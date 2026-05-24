using UnityEngine;
using System.Collections.Generic;
//using System.Drawing;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    public int NumberOfColumns = 10;
    public int NumberOfRows = 8;
    public float WorldTileSize = 1f;

    [Header("Debug")]
    [SerializeField] private float tileGizmoAlpha = 0.4f;
    [SerializeField] private bool showTilesInPlayMode = false;
    [SerializeField] private bool showGridCoordinates;
    // Serialized so the tile data is saved with the scene and visible to the custom editor
    [SerializeField] private GroundTileData[] serializedTileGridArray;
    private List<GameObject> debugTileObjects = new List<GameObject>();
    private Dictionary<Vector2Int, SpriteRenderer> debugTileRenderers = new Dictionary<Vector2Int, SpriteRenderer>();


    private GroundTileData[,] tileGrid;

    [Header("Prefabs")]
    [SerializeField] private GameObject pressurePlatePrefab;
    [SerializeField] private GameObject runeBeamVisualPrefab;

    // Fired when a pressure plate is activated or deactivated: (column, row, isActivated)
    public event System.Action<int, int, bool> OnPressurePlateStateChanged;


    void Awake()
    {
        Instance = this;
        InitialiseGrid();
    }

    private void Start()
    {
        SubscribeToRuneEvents();
    }



    void InitialiseGrid()
    {
        tileGrid = new GroundTileData[NumberOfColumns, NumberOfRows];

        for (int column = 0; column < NumberOfColumns; column++)
            for (int row = 0; row < NumberOfRows; row++)
                tileGrid[column, row] = GetSerializedTileAt(column, row);
    }


    // Reads from the serializedTileGrid, falls back to Hedge if not yet painted
    public GroundTileData GetSerializedTileAt(int column, int row)
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

                if (previousGrid != null && index < previousGrid.Length)
                    serializedTileGridArray[index] = previousGrid[index];
                else
                    serializedTileGridArray[index] = new GroundTileData { GroundTileType = GroundTileTypeEnum.Hedge };
            }
        }
    }


    // Called by the editor script when a cell is painted
    public void PaintGroundTile(int column, int row, GroundTileTypeEnum groundTileType)
    {
        if (!IsCellInBounds(column, row)) return;

        int index = row * NumberOfColumns + column;

        if (serializedTileGridArray == null || index >= serializedTileGridArray.Length)
            RebuildSerializedGrid();

        serializedTileGridArray[index].GroundTileType = groundTileType;
    }

    public void PaintChannelOverlay(int column, int row, RuneChannelTypeEnum channel)
    {
        if (!IsCellInBounds(column, row)) return;
        int index = row * NumberOfColumns + column;
        if (serializedTileGridArray == null || index >= serializedTileGridArray.Length) return;
        serializedTileGridArray[index].RuneChannel = channel;
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
            int index = row * NumberOfColumns + column;
            if (serializedTileGridArray != null && index < serializedTileGridArray.Length)
            {
                serializedTileGridArray[index].GroundTileType = GroundTileTypeEnum.Stone;
                serializedTileGridArray[index].IsOccupiedByMoveable = false;
                serializedTileGridArray[index].Interactable = null;
                UnityEngine.Color color = TileEditorColors.GetColorForTileType(tile.GroundTileType);
                SpriteRenderer spriteRenderer = debugTileObjects[index].GetComponent<SpriteRenderer>();
                spriteRenderer.color = color;

                //repaint the tile in the editor so it doesn't look like a water tile anymore
                //TODO: replaceSprite so it shows bridge type thing
                return;
            }

        }

        if (!tile.IsInWater)
        {
            tile.IsOccupiedByMoveable = isOccupied;
        }


        // Pressure plate activation
        if (tile.GroundTileType == GroundTileTypeEnum.PressurePlate)
        {
            bool wasActivated = tile.IsPressurePlateActivated;
            tile.IsPressurePlateActivated = isOccupied;

            if (tile.IsPressurePlateActivated != wasActivated)
                OnPressurePlateStateChanged?.Invoke(column, row, tile.IsPressurePlateActivated);

            //todo: we could also invoke this event when the player steps on/off a pressure plate,
            //but currently we only have pressure plates that interact with rocks,
            //so it's simpler to just trigger on moveable occupancy changes
        }
    }

    public void SetDoorState(int column, int row, bool isOpen)
    {
        if (!IsCellInBounds(column, row)) return;
        tileGrid[column, row].IsDoorOpen = isOpen;
        tileGrid[column, row].GroundTileType = GroundTileTypeEnum.Door;
    }

    public GroundTileData GetTileAt(int column, int row)
    {
        if (!IsCellInBounds(column, row)) return null;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            int index = row * NumberOfColumns + column;
            if (serializedTileGridArray == null || index >= serializedTileGridArray.Length) return null;
            return serializedTileGridArray[index];
        }
#endif

        return tileGrid[column, row];
    }

    public void RegisterRotatableRuneBlock(int column, int row, RotatableRuneBlock block)
    {
        if (!IsCellInBounds(column, row)) return;
        tileGrid[column, row].RotatableRuneBlock = block;
    }

    public RotatableRuneBlock GetRotatableRuneBlockAt(int column, int row)
    {
        if (!IsCellInBounds(column, row)) return null;
        return tileGrid[column, row].RotatableRuneBlock;
    }

    public void RegisterInteractable(int column, int row, IInteractable interactable)
    {
        if (!IsCellInBounds(column, row)) return;
        tileGrid[column, row].Interactable = interactable;
    }

    public IInteractable GetInteractableAtGridPosition(int column, int row)
    {
        if (!IsCellInBounds(column, row)) return null;
        return tileGrid[column, row].Interactable;
    }


    public bool IsCellInBounds(int column, int row)
    {
        return column >= 0 && row >= 0 && column < NumberOfColumns && row < NumberOfRows;
    }


    public bool IsCellPassableByPlayer(int column, int row)
    {
        GroundTileData tile = GetTileAt(column, row);
        //Debug.Log(tile.IsPassableByPlayer + " , " + !tile.IsOccupiedByMoveable);
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
        return transform.position + new Vector3(
            column * WorldTileSize + WorldTileSize * 0.5f,
            row * WorldTileSize + WorldTileSize * 0.5f,
            0f
        );
    }





    void OnEnable()
    {
        if (Application.isPlaying && showTilesInPlayMode)
            CreateDebugTiles();
    }

    void OnDisable()
    {
        DestroyDebugTiles();
    }

    public void SubscribeToRuneEvents()
    {
        if (RunePowerSystem.Instance == null)
        {
            Debug.LogWarning("GridManager: RunePowerSystem.Instance is null during SubscribeToRuneEvents");
            return;
        }

        //Debug.Log($"GridManager: Subscribing to rune events. Renderer dict has {debugTileRenderers.Count} entries.");
        RunePowerSystem.Instance.OnTileRunePowerChanged += HandleTileRunePowerChanged;
    }

    void HandleTileRunePowerChanged(int column, int row, bool isPowered)
    {
        Vector2Int key = new Vector2Int(column, row);

        //Debug.Log($"GridManager: HandleTileRunePowerChanged ({column},{row}) powered={isPowered}. Dict has {debugTileRenderers.Count} entries. ContainsKey={debugTileRenderers.ContainsKey(key)}");

        if (!debugTileRenderers.ContainsKey(key)) return;

        SpriteRenderer spriteRenderer = debugTileRenderers[key];
        GroundTileData tile = GetTileAt(column, row);
        if (tile == null) return;

        Color color = isPowered
            ? new Color(1.0f, 0.9f, 0.2f, tileGizmoAlpha)
            : TileEditorColors.GetColorForTileType(tile.GroundTileType);

        color.a = tileGizmoAlpha;
        spriteRenderer.color = color;
    }

    void CreateDebugTiles()
    {
        DestroyDebugTiles();

        //Debug.Log("GridManager: CreateDebugTiles called");

        if (serializedTileGridArray == null) return;

        for (int column = 0; column < NumberOfColumns; column++)
        {
            for (int row = 0; row < NumberOfRows; row++)
            {
                int index = row * NumberOfColumns + column;
                if (index >= serializedTileGridArray.Length) continue;

                GroundTileData tile = serializedTileGridArray[index];
                Color color = TileEditorColors.GetColorForTileType(tile.GroundTileType);
                color.a = tileGizmoAlpha;

                GameObject debugTile = new GameObject($"DebugTile_{column}_{row}");
                debugTile.transform.SetParent(transform);
                debugTile.transform.position = ConvertGridPositionToWorldPosition(column, row);
                debugTile.transform.localScale = Vector3.one * (WorldTileSize * 0.9f);

                SpriteRenderer spriteRenderer = debugTile.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateWhiteSquareSprite();
                spriteRenderer.color = color;
                spriteRenderer.sortingOrder = -10;

                Vector2Int key = new Vector2Int(column, row);
                debugTileRenderers[key] = spriteRenderer;
                debugTileObjects.Add(debugTile);
            }
        }
        SpawnPressurePlates();
        SpawnRuneBeamVisuals();
    }

    void DestroyDebugTiles()
    {
        if (RunePowerSystem.Instance != null)
            RunePowerSystem.Instance.OnTileRunePowerChanged -= HandleTileRunePowerChanged;

        foreach (GameObject debugTile in debugTileObjects)
        {
            if (debugTile != null)
                Destroy(debugTile);
        }

        debugTileObjects.Clear();
        debugTileRenderers.Clear();
    }

    Sprite CreateWhiteSquareSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );
    }
    void SpawnPressurePlates()
    {
        if (pressurePlatePrefab == null) return;

        for (int column = 0; column < NumberOfColumns; column++)
        {
            for (int row = 0; row < NumberOfRows; row++)
            {
                int index = row * NumberOfColumns + column;
                if (index >= serializedTileGridArray.Length) continue;

                if (serializedTileGridArray[index].GroundTileType != GroundTileTypeEnum.PressurePlate) continue;

                Vector3 worldPos = ConvertGridPositionToWorldPosition(column, row);
                GameObject plate = Instantiate(pressurePlatePrefab, worldPos, Quaternion.identity, transform);
                plate.name = $"PressurePlate_{column}_{row}";

                PressurePlateVisual pp = plate.GetComponent<PressurePlateVisual>();
                if (pp != null)
                    pp.Initialise(column, row);
            }
        }
    }
    public void RegisterReceiver(int column, int row, RuneReceiver receiver)
    {
        if (!IsCellInBounds(column, row)) return;
        tileGrid[column, row].Receiver = receiver;
    }

    public RuneReceiver GetReceiverAt(int column, int row)
    {
        if (!IsCellInBounds(column, row)) return null;
        return tileGrid[column, row].Receiver;
    }


    public void RegisterBeamVisual(int column, int row, RuneBeamVisual beam)
    {
        if (!IsCellInBounds(column, row)) return;
        tileGrid[column, row].BeamVisual = beam;
    }

    public RuneBeamVisual GetBeamVisualAt(int column, int row)
    {
        if (!IsCellInBounds(column, row)) return null;
        return tileGrid[column, row].BeamVisual;
    }


    void SpawnRuneBeamVisuals()
    {
        if (runeBeamVisualPrefab == null)
        {
            Debug.LogWarning("GridManager: runeBeamVisualPrefab is not assigned.");
            return;
        }

        for (int column = 0; column < NumberOfColumns; column++)
        {
            for (int row = 0; row < NumberOfRows; row++)
            {
                int serializedIndex = row * NumberOfColumns + column;
                if (serializedIndex >= serializedTileGridArray.Length) continue;

                GroundTileData tileData = serializedTileGridArray[serializedIndex];
                if (tileData.RuneChannel == RuneChannelTypeEnum.None) continue;

                Vector3 worldPosition = ConvertGridPositionToWorldPosition(column, row);
                GameObject spawnedBeamObject = Instantiate(runeBeamVisualPrefab, worldPosition, Quaternion.identity, transform);
                spawnedBeamObject.name = $"RuneBeamVisual_{column}_{row}";
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        DrawPaintedTilesWithHandles();
        DrawCellGrid();
        DrawGridBounds();
        DrawOriginMarker();

        if (showGridCoordinates)
            DrawGridCoordinates();
    }

    void DrawPaintedTilesWithHandles()
    {
        if (serializedTileGridArray == null) return;

        for (int column = 0; column < NumberOfColumns; column++)
        {
            for (int row = 0; row < NumberOfRows; row++)
            {
                int index = row * NumberOfColumns + column;
                if (index >= serializedTileGridArray.Length) continue;

                GroundTileData tile = serializedTileGridArray[index];
                float halfSize = WorldTileSize * 0.45f;
                Vector3 center = transform.position + new Vector3(
                    column * WorldTileSize + WorldTileSize * 0.5f,
                    row * WorldTileSize + WorldTileSize * 0.5f,
                    0f
                );

                Vector3[] corners = new Vector3[]
                {
                    center + new Vector3(-halfSize, -halfSize, 0),
                    center + new Vector3( halfSize, -halfSize, 0),
                    center + new Vector3( halfSize,  halfSize, 0),
                    center + new Vector3(-halfSize,  halfSize, 0),
                };

                Color tileColor = TileEditorColors.GetColorForTileType(tile.GroundTileType);
                tileColor.a = tileGizmoAlpha;

                UnityEditor.Handles.DrawSolidRectangleWithOutline(corners, tileColor, Color.clear);
            }
        }
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

    void DrawGridCoordinates()
    {
        GUIStyle labelStyle = new GUIStyle();
        labelStyle.fontSize = 10;
        labelStyle.normal.textColor = new Color(1f, 1f, 1f, 0.7f);
        labelStyle.alignment = TextAnchor.MiddleCenter;

        for (int column = 0; column < NumberOfColumns; column++)
        {
            for (int row = 0; row < NumberOfRows; row++)
            {
                Vector3 center = transform.position + new Vector3(
                    column * WorldTileSize + WorldTileSize * 0.5f,
                    row * WorldTileSize + WorldTileSize * 0.5f,
                    0f
                );

                UnityEditor.Handles.Label(center, $"{column},{row}", labelStyle);
            }
        }
    }

#endif
}