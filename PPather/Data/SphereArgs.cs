using System.Numerics;

namespace PPather.Data;

public sealed class SphereArgs
{
    public string Name { get; set; }
    public Vector3 Spot { get; set; }
    public int Colour { get; set; }
    public int MapId { get; set; }

    public SphereArgs(string name, Vector3 spot, int colour, int mapId)
    {
        Name = name;
        Spot = spot;
        Colour = colour;
        MapId = mapId;
    }
}