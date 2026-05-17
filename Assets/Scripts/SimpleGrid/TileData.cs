




[System.Serializable]
public class TileData
{
    public TileTypeEnum TileType;
    public bool[] ActiveConnections = new bool[4]; // North, East, South, West
    public bool IsCurrentlyPowered;

    // Connector only
    public ConnectorShapeEnum ConnectorShape;
    public int CurrentRotationStep; // 0–3, clockwise

    // Switch only
    public bool IsSwitchOn = true;
}
