using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    private GroundTileTypeEnum selectedGroundBrush = GroundTileTypeEnum.Grass;
    private RuneChannelTypeEnum selectedChannelBrush = RuneChannelTypeEnum.None;


    private readonly string[] brushLabels = System.Enum.GetNames(typeof(GroundTileTypeEnum));
    private readonly string[] channelLabels = System.Enum.GetNames(typeof(RuneChannelTypeEnum));
    private Color[] groundBrushColors;
    private Color[] channelBrushColors;

    private enum PaintMode { Ground, Channel }
    private PaintMode paintMode = PaintMode.Ground;


    void OnEnable()
    {
        System.Array groundEnumValues = System.Enum.GetValues(typeof(GroundTileTypeEnum));
        System.Array channelEnumValues = System.Enum.GetValues(typeof(RuneChannelTypeEnum));
        groundBrushColors = new Color[groundEnumValues.Length];
        channelBrushColors = new Color[channelEnumValues.Length];

        for (int i = 0; i < groundEnumValues.Length; i++)
            groundBrushColors[i] = TileEditorColors.GetColorForTileType((GroundTileTypeEnum)groundEnumValues.GetValue(i));

        for (int i = 0; i < channelEnumValues.Length; i++)
            channelBrushColors[i] = TileEditorColors.GetColorForTileType((RuneChannelTypeEnum)channelEnumValues.GetValue(i));
    }





    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GridManager gridManager = (GridManager)target;

        EditorGUI.BeginChangeCheck();

        // Grid size fields
        int newColumns = EditorGUILayout.IntField("Number Of Columns", gridManager.NumberOfColumns);
        int newRows = EditorGUILayout.IntField("Number Of Rows", gridManager.NumberOfRows);
        float newTileSize = EditorGUILayout.FloatField("World Tile Size", gridManager.WorldTileSize);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(gridManager, "Resize Grid");
            gridManager.NumberOfColumns = Mathf.Max(1, newColumns);
            gridManager.NumberOfRows = Mathf.Max(1, newRows);
            gridManager.WorldTileSize = Mathf.Max(0.1f, newTileSize);
            gridManager.RebuildSerializedGrid();
            EditorUtility.SetDirty(gridManager);
        }

        paintMode = (PaintMode)GUILayout.Toolbar((int)paintMode, new[] { "Paint Ground", "Paint Channel" });
        EditorGUILayout.Space();

        EditorGUILayout.Space();


        if (paintMode == PaintMode.Ground)
        {
            EditorGUILayout.LabelField("Ground Brush", EditorStyles.boldLabel);
            // Row 1 — ground tiles
            DrawGroundBrushRow(new GroundTileTypeEnum[]
            {
            GroundTileTypeEnum.Hedge,
            GroundTileTypeEnum.Grass,
            GroundTileTypeEnum.Stone,
            GroundTileTypeEnum.Water,
            GroundTileTypeEnum.PressurePlate,
            });

            // Row 2 — door and rune tiles
            DrawGroundBrushRow(new GroundTileTypeEnum[]
            {
            GroundTileTypeEnum.Door,
            GroundTileTypeEnum.RuneSource,
            GroundTileTypeEnum.RuneReceiver,
            });

        }

        else
        {
            EditorGUILayout.LabelField("Channel Overlay Brush", EditorStyles.boldLabel);
            DrawChannelBrushRow(new RuneChannelTypeEnum[]
            {
            RuneChannelTypeEnum.Horizontal,
            RuneChannelTypeEnum.Vertical,
            RuneChannelTypeEnum.None,
            });
        }




        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Click or drag in the Scene view to paint tiles.", MessageType.Info);

        //if (GUILayout.Button("Fill All With Brush"))
        //{
        //    Undo.RecordObject(gridManager, "Fill Grid");
        //    FillAll(gridManager, selectedGroundBrush);
        //    EditorUtility.SetDirty(gridManager);
        //}

        if (GUILayout.Button("Reset To Hedge"))
        {
            Undo.RecordObject(gridManager, "Reset Grid");
            FillAll(gridManager, GroundTileTypeEnum.Hedge);
            EditorUtility.SetDirty(gridManager);
        }
    }

    void DrawGroundBrushRow(GroundTileTypeEnum[] tileTypes)
    {
        EditorGUILayout.BeginHorizontal();

        foreach (GroundTileTypeEnum tileType in tileTypes)
        {
            int index = (int)tileType;
            bool isSelected = selectedGroundBrush == tileType;

            GUI.backgroundColor = index < groundBrushColors.Length ? groundBrushColors[index] : Color.white;
            GUIStyle buttonStyle = isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton;

            if (GUILayout.Toggle(isSelected, brushLabels[index], buttonStyle))
                selectedGroundBrush = tileType;
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }

    void DrawChannelBrushRow(RuneChannelTypeEnum[] tileTypes)
    {
        EditorGUILayout.BeginHorizontal();

        foreach (RuneChannelTypeEnum tileType in tileTypes)
        {
            int index = (int)tileType;
            bool isSelected = selectedChannelBrush == tileType;

            GUI.backgroundColor = index < channelBrushColors.Length ? channelBrushColors[index] : Color.white;
            GUIStyle buttonStyle = isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton;

            if (GUILayout.Toggle(isSelected, channelLabels[index], buttonStyle))
                selectedChannelBrush = tileType;
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }



    void OnSceneGUI()
    {
        GridManager gridManager = (GridManager)target;

        if (Event.current.type == EventType.Repaint)
            DrawChannelOverlayGizmos(gridManager);

        Event currentEvent = Event.current;

        // Consume left-click and drag so Unity doesn't deselect the GridManager
        if (currentEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        if ((currentEvent.type == EventType.MouseDown || currentEvent.type == EventType.MouseDrag)
            && currentEvent.button == 0)
        {
            Vector2Int? cellUnderMouse = GetCellUnderMouse(gridManager, currentEvent);

            if (cellUnderMouse.HasValue)
            {
                Undo.RecordObject(gridManager, "Paint Tile");

                if (paintMode == PaintMode.Ground)
                {
                    gridManager.PaintGroundTile(cellUnderMouse.Value.x, cellUnderMouse.Value.y, selectedGroundBrush);
                }

                else
                {
                    gridManager.PaintChannelOverlay(cellUnderMouse.Value.x, cellUnderMouse.Value.y, selectedChannelBrush);
                }

                EditorUtility.SetDirty(gridManager);
                currentEvent.Use();
            }
        }

        // Draw brush label in scene view so the designer knows what they are painting
        Handles.BeginGUI();
        GUI.color = Color.white;
        string brushInfo = paintMode == PaintMode.Ground? $"Ground: {selectedGroundBrush}" : $"Channel: {selectedChannelBrush}";
        GUI.Label(new Rect(10, 10, 200, 24), brushInfo);
        Handles.EndGUI();
    }


    Vector2Int? GetCellUnderMouse(GridManager gridManager, Event currentEvent)
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
        Vector3 worldPoint = mouseRay.origin;
        Vector3 local = worldPoint - gridManager.transform.position;

        int column = Mathf.FloorToInt(local.x / gridManager.WorldTileSize);
        int row = Mathf.FloorToInt(local.y / gridManager.WorldTileSize);

        if (!gridManager.IsCellInBounds(column, row)) return null;

        return new Vector2Int(column, row);
    }


    void FillAll(GridManager gridManager, GroundTileTypeEnum tileType)
    {
        gridManager.RebuildSerializedGrid();

        for (int column = 0; column < gridManager.NumberOfColumns; column++)
            for (int row = 0; row < gridManager.NumberOfRows; row++)
                gridManager.PaintGroundTile(column, row, tileType);
    }

    void DrawChannelOverlayGizmos(GridManager gridManager)
    {
        if (gridManager == null) return;
        if (gridManager.NumberOfColumns == 0 || gridManager.NumberOfRows == 0) return;


        float tileSize = gridManager.WorldTileSize;
        float thickness = tileSize * 0.2f;
        Vector3 origin = gridManager.transform.position;

        // Debug: draw a red dot at grid origin so we know the method is firing
        Handles.color = Color.red;
        Handles.DrawSolidDisc(origin, Vector3.forward, 0.1f);

        for (int col = 0; col < gridManager.NumberOfColumns; col++)
        {
            for (int row = 0; row < gridManager.NumberOfRows; row++)
            {
                GroundTileData tile = gridManager.GetTileAt(col, row);

                // Debug: draw a small white dot at every tile center
                Vector3 center = origin + new Vector3(
                    col * tileSize + tileSize * 0.5f,
                    row * tileSize + tileSize * 0.5f,
                    0f
                );
                Handles.color = Color.white;
                Handles.DrawSolidDisc(center, Vector3.forward, tileSize * 0.05f);

                if (tile == null || tile.RuneChannel == RuneChannelTypeEnum.None) continue;


                Handles.color = tile.RuneChannel == RuneChannelTypeEnum.Horizontal
                    ? TileEditorColors.RuneChannelHorizontal
                    : TileEditorColors.RuneChannelVertical;

                Vector3 size = tile.RuneChannel == RuneChannelTypeEnum.Horizontal
                    ? new Vector3(tileSize, thickness, 0f)
                    : new Vector3(thickness, tileSize, 0f);

                Color fillColor = Handles.color;
                fillColor.a = 0.55f;

                Handles.DrawSolidRectangleWithOutline(
                    GetRectVerts(center, size),
                    fillColor,
                    Color.clear
                );
            }
        }

        Handles.color = Color.white;
    }

    Vector3[] GetRectVerts(Vector3 center, Vector3 size)
    {
        float hw = size.x * 0.5f;
        float hh = size.y * 0.5f;
        return new Vector3[]
        {
        center + new Vector3(-hw, -hh, 0f),
        center + new Vector3( hw, -hh, 0f),
        center + new Vector3( hw,  hh, 0f),
        center + new Vector3(-hw,  hh, 0f),
        };
    }

}