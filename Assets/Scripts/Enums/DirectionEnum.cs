using UnityEngine;

public enum DirectionEnum
{
    North,
    East,
    South,
    West,
}

public static class DirectionEnumExtensions
{
    public static Vector2Int ToVector(this DirectionEnum direction)
    {
        switch (direction)
        {
            case DirectionEnum.North: return Vector2Int.up;
            case DirectionEnum.East: return Vector2Int.right;
            case DirectionEnum.South: return Vector2Int.down;
            case DirectionEnum.West: return Vector2Int.left;
            default: return Vector2Int.zero;
        }
    }

    public static DirectionEnum Opposite(this DirectionEnum direction)
    {
        switch (direction)
        {
            case DirectionEnum.North: return DirectionEnum.South;
            case DirectionEnum.East: return DirectionEnum.West;
            case DirectionEnum.South: return DirectionEnum.North;
            case DirectionEnum.West: return DirectionEnum.East;
            default: return DirectionEnum.North;
        }
    }
}