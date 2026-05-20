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
    RuneChannel,   // fixed rune path, always connects in all directions walkable
    RuneReceiver,  // target that triggers when powered
}