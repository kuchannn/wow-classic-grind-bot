using System.Numerics;

namespace PPather.Data;

public sealed class LinesEventArgs
{
    public Vector4[] Locations { get; set; }
    public int Colour { get; set; }
    public string Name { get; set; }

    public LinesEventArgs(string name, Vector4[] locations, int colour)
    {
        this.Name = name;
        this.Locations = locations;
        this.Colour = colour;
    }
}