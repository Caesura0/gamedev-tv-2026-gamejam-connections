[System.Serializable]
public class GroundTileData
{
    public GroundTileTypeEnum GroundTileType;
    public bool IsOccupiedByMoveable;
    public bool IsPressurePlateActivated;

    public bool IsPassableByPlayer =>
        GroundTileType == GroundTileTypeEnum.Grass ||
        GroundTileType == GroundTileTypeEnum.Stone ||
        GroundTileType == GroundTileTypeEnum.PressurePlate;

    public bool IsValidRockDestination =>
        (GroundTileType == GroundTileTypeEnum.Stone ||
         GroundTileType == GroundTileTypeEnum.PressurePlate ||
         GroundTileType == GroundTileTypeEnum.Water)
        && !IsOccupiedByMoveable;
}