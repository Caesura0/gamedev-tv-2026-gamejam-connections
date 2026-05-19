[System.Serializable]
public class GroundTileData
{
    public GroundTileTypeEnum GroundTileType;
    public bool IsOccupiedByMoveable;
    public bool IsPressurePlateActivated;
    public bool IsInWater;
    public bool IsRunePowered;
    [System.NonSerialized] public IInteractable Interactable;
    [System.NonSerialized] public RotatableRuneBlock RotatableRuneBlock;

    public bool IsDoorOpen;

    public bool IsPassableByPlayer =>
        (GroundTileType == GroundTileTypeEnum.Door && IsDoorOpen) ||
        GroundTileType == GroundTileTypeEnum.Grass ||
        GroundTileType == GroundTileTypeEnum.Stone ||
        GroundTileType == GroundTileTypeEnum.RuneChannelVertical ||
        GroundTileType == GroundTileTypeEnum.RuneChannelHorizontal ||
        GroundTileType == GroundTileTypeEnum.PressurePlate;


    public bool IsValidRockDestination =>
        (GroundTileType == GroundTileTypeEnum.Stone ||
         GroundTileType == GroundTileTypeEnum.PressurePlate ||
         GroundTileType == GroundTileTypeEnum.Water)
        && !IsOccupiedByMoveable;
}