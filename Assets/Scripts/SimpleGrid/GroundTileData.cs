[System.Serializable]
public class GroundTileData
{
    public GroundTileTypeEnum GroundTileType;
    public bool IsOccupiedByMoveable;
    public bool IsPressurePlateActivated;
    public bool IsInWater;
    [System.NonSerialized] public IInteractable Interactable;

    public bool IsDoorOpen;

    public bool IsPassableByPlayer =>
        (GroundTileType == GroundTileTypeEnum.Door && IsDoorOpen) ||
        GroundTileType == GroundTileTypeEnum.Grass ||
        GroundTileType == GroundTileTypeEnum.Stone ||
        GroundTileType == GroundTileTypeEnum.PressurePlate;


    public bool IsValidRockDestination =>
        (GroundTileType == GroundTileTypeEnum.Stone ||
         GroundTileType == GroundTileTypeEnum.PressurePlate ||
         GroundTileType == GroundTileTypeEnum.Water)
        && !IsOccupiedByMoveable;
}