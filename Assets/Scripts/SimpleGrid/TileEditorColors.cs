using UnityEngine;

public static class TileEditorColors
{
    public static readonly Color Hedge = new Color(0.05f, 0.20f, 0.05f);
    public static readonly Color Grass = new Color(0.15f, 0.85f, 0.10f);
    public static readonly Color Stone = new Color(0.60f, 0.60f, 0.60f);
    public static readonly Color Water = new Color(0.10f, 0.40f, 0.90f);
    public static readonly Color PressurePlate = new Color(0.85f, 0.75f, 0.10f);
    public static readonly Color Door = new Color(0.70f, 0.35f, 0.10f);

    // Rune layer
    public static readonly Color RuneSource = new Color(0.90f, 0.20f, 0.90f);
    public static readonly Color RuneChannelHorizontal = new Color(0.60f, 0.20f, 0.80f);
    public static readonly Color RuneChannelVertical = new Color(0.50f, 0.15f, 0.70f);
    public static readonly Color RuneReceiver = new Color(0.20f, 0.80f, 0.90f);

    public static Color GetColorForTileType(GroundTileTypeEnum tileType) => tileType switch
    {
        GroundTileTypeEnum.Hedge => Hedge,
        GroundTileTypeEnum.Grass => Grass,
        GroundTileTypeEnum.Stone => Stone,
        GroundTileTypeEnum.Water => Water,
        GroundTileTypeEnum.PressurePlate => PressurePlate,
        GroundTileTypeEnum.Door => Door,
        GroundTileTypeEnum.RuneSource => RuneSource,
        GroundTileTypeEnum.RuneChannelHorizontal => RuneChannelHorizontal,
        GroundTileTypeEnum.RuneChannelVertical => RuneChannelVertical,
        GroundTileTypeEnum.RuneReceiver => RuneReceiver,
        _ => Color.black,
    };
}