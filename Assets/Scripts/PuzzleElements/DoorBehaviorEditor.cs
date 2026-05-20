using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DoorBehaviour))]
public class DoorBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        DoorBehaviour door = (DoorBehaviour)target;
        GridManager gridManager = FindAnyObjectByType<GridManager>();

        serializedObject.Update();
        SerializedProperty requiredPositions = serializedObject.FindProperty("requiredPlatePositions");

        EditorGUILayout.LabelField("Required Pressure Plates", EditorStyles.boldLabel);

        if (gridManager == null)
        {
            EditorGUILayout.HelpBox("No GridManager found in the scene.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        // Collect all pressure plate positions from the grid
        bool anyPlatesFound = false;

        for (int row = gridManager.NumberOfRows - 1; row >= 0; row--)
        {
            for (int column = 0; column < gridManager.NumberOfColumns; column++)
            {
                GroundTileData tile = gridManager.GetSerializedTileAt(column, row);
                if (tile == null || tile.GroundTileType != GroundTileTypeEnum.PressurePlate) continue;

                anyPlatesFound = true;
                Vector2Int position = new Vector2Int(column, row);
                bool isRequired = IsPositionRequired(requiredPositions, position);

                bool newValue = EditorGUILayout.ToggleLeft(
                    $"Plate at ({column}, {row})",
                    isRequired
                );

                if (newValue == isRequired) continue;

                Undo.RecordObject(door, "Toggle Required Plate");

                if (newValue)
                    AddPosition(requiredPositions, position);
                else
                    RemovePosition(requiredPositions, position);
            }
        }

        if (!anyPlatesFound)
            EditorGUILayout.HelpBox("No pressure plate tiles found in the grid.", MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    bool IsPositionRequired(SerializedProperty list, Vector2Int position)
    {
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            if (element.FindPropertyRelative("x").intValue == position.x &&
                element.FindPropertyRelative("y").intValue == position.y)
                return true;
        }
        return false;
    }

    void AddPosition(SerializedProperty list, Vector2Int position)
    {
        list.arraySize++;
        SerializedProperty newElement = list.GetArrayElementAtIndex(list.arraySize - 1);
        newElement.FindPropertyRelative("x").intValue = position.x;
        newElement.FindPropertyRelative("y").intValue = position.y;
    }

    void RemovePosition(SerializedProperty list, Vector2Int position)
    {
        for (int i = 0; i < list.arraySize; i++)
        {
            SerializedProperty element = list.GetArrayElementAtIndex(i);
            if (element.FindPropertyRelative("x").intValue != position.x ||
                element.FindPropertyRelative("y").intValue != position.y) continue;

            list.DeleteArrayElementAtIndex(i);
            return;
        }
    }
}