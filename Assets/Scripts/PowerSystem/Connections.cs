using System.Collections.Generic;
using UnityEngine;

public static class Connections
{
    // [shape][rotation] → bool[4] N,E,S,W
    static readonly Dictionary<ConnectorShapeEnum, bool[][]> Table = new()
    {
        { ConnectorShapeEnum.Straight, new[] {
            new[] { true,  false, true,  false },  // 0: N-S
            new[] { false, true,  false, true  },  // 1: E-W
            new[] { true,  false, true,  false },  // 2: N-S
            new[] { false, true,  false, true  },  // 3: E-W
        }},
        { ConnectorShapeEnum.Elbow, new[] {
            new[] { true,  true,  false, false },  // 0: N-E
            new[] { false, true,  true,  false },  // 1: E-S
            new[] { false, false, true,  true  },  // 2: S-W
            new[] { true,  false, false, true  },  // 3: W-N
        }},
        { ConnectorShapeEnum.Tee, new[] {
            new[] { true,  true,  false, true  },  // 0: N-E-W
            new[] { true,  true,  true,  false },  // 1: N-E-S
            new[] { false, true,  true,  true  },  // 2: E-S-W
            new[] { true,  false, true,  true  },  // 3: N-S-W
        }},
    };

    static readonly Vector2Int[] Vectors =
    {
        new( 0,  1),  // North
        new( 1,  0),  // East
        new( 0, -1),  // South
        new(-1,  0),  // West
    };

    public static bool[] Get(ConnectorShapeEnum shape, int rotation) => Table[shape][rotation % 4];
    public static Vector2Int Vec(DirectionEnum d) => Vectors[(int)d];
    public static DirectionEnum Opp(DirectionEnum d) => (DirectionEnum)(((int)d + 2) % 4);
}





