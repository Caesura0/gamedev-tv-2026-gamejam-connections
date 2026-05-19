using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    private GroundTileTypeEnum selectedBrush = GroundTileTypeEnum.Grass;



    private readonly string[] brushLabels = System.Enum.GetNames(typeof(GroundTileTypeEnum));
    private Color[] brushColors;




    void OnEnable()
    {
        System.Array enumValues = System.Enum.GetValues(typeof(GroundTileTypeEnum));
        brushColors = new Color[enumValues.Length];

        for (int i = 0; i < enumValues.Length; i++)
            brushColors[i] = TileEditorColors.GetColorForTileType((GroundTileTypeEnum)enumValues.GetValue(i));
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

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Paint Brush", EditorStyles.boldLabel);

        // Row 1 — ground tiles
        DrawBrushRow(new GroundTileTypeEnum[]
        {
            GroundTileTypeEnum.Hedge,
            GroundTileTypeEnum.Grass,
            GroundTileTypeEnum.Stone,
            GroundTileTypeEnum.Water,
            GroundTileTypeEnum.PressurePlate,
        });

        // Row 2 — door and rune tiles
        DrawBrushRow(new GroundTileTypeEnum[]
        {
            GroundTileTypeEnum.Door,
            GroundTileTypeEnum.RuneSource,
            GroundTileTypeEnum.RuneChannelHorizontal,
            GroundTileTypeEnum.RuneChannelVertical,
            GroundTileTypeEnum.RuneReceiver,
        });

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Click or drag in the Scene view to paint tiles.", MessageType.Info);

        if (GUILayout.Button("Fill All With Brush"))
        {
            Undo.RecordObject(gridManager, "Fill Grid");
            FillAll(gridManager, selectedBrush);
            EditorUtility.SetDirty(gridManager);
        }

        if (GUILayout.Button("Reset To Hedge"))
        {
            Undo.RecordObject(gridManager, "Reset Grid");
            FillAll(gridManager, GroundTileTypeEnum.Hedge);
            EditorUtility.SetDirty(gridManager);
        }
    }

    void DrawBrushRow(GroundTileTypeEnum[] tileTypes)
    {
        EditorGUILayout.BeginHorizontal();

        foreach (GroundTileTypeEnum tileType in tileTypes)
        {
            int index = (int)tileType;
            bool isSelected = selectedBrush == tileType;

            GUI.backgroundColor = index < brushColors.Length ? brushColors[index] : Color.white;
            GUIStyle buttonStyle = isSelected ? EditorStyles.miniButtonMid : EditorStyles.miniButton;

            if (GUILayout.Toggle(isSelected, brushLabels[index], buttonStyle))
                selectedBrush = tileType;
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();
    }

    void OnSceneGUI()
    {
        GridManager gridManager = (GridManager)target;

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
                gridManager.PaintTile(cellUnderMouse.Value.x, cellUnderMouse.Value.y, selectedBrush);
                EditorUtility.SetDirty(gridManager);
                currentEvent.Use();
            }
        }

        // Draw brush label in scene view so the designer knows what they are painting
        Handles.BeginGUI();
        GUI.color = Color.white;
        GUI.Label(new Rect(10, 10, 200, 24), $"Brush: {selectedBrush}");
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
                gridManager.PaintTile(column, row, tileType);
    }

}