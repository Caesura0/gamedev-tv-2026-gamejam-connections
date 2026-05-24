[System.Serializable]
public class GroundTileData
{
    public GroundTileTypeEnum GroundTileType;
    public RuneChannelTypeEnum RuneChannel;
    public bool IsOccupiedByMoveable;
    public bool IsPressurePlateActivated;
    public bool IsInWater;
    public bool IsRunePowered;
    public bool IsDoorOpen;

    [System.NonSerialized] public IInteractable Interactable;
    [System.NonSerialized] public RotatableRuneBlock RotatableRuneBlock;
    [System.NonSerialized] public RuneReceiver Receiver;
    [System.NonSerialized] public RuneBeamVisual BeamVisual;

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