using System.Numerics;

namespace PPather.Data;

public sealed class LineArgs
{
    public string Name { get; set; }
    public Vector3[] Spots { get; set; }
    public int Colour { get; set; }
    public int MapId { get; set; }

    public LineArgs(string name, Vector3[] spots, int colour, int mapId)
    {
        Name = name;
        Spots = spots;
        Colour = colour;
        MapId = mapId;
    }
}