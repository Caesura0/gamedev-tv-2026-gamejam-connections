public enum GroundTileTypeEnum
{
    Hedge,         // impassable
    Grass,         // passable by player
    Stone,         // passable by player; required for rock movement
    Water,         // impassable; rock can be pushed onto it, converting it to stone
    PressurePlate, // passable by player; activates when rock is on it
    Door,          // impassable when closed, passable when open

    // Rune layer
    RuneSource,    // emits rune power
    RuneChannelHorizontal, // fixed rune path, connects East-West only
    RuneChannelVertical,   // fixed rune path, connects North-South only
    RuneReceiver,  // target that triggers when powered
}