using System.Numerics;

namespace PPather.Data;

public sealed class SphereEventArgs
{
    public Vector4 Location { get; set; }
    public int Colour { get; set; }
    public string Name { get; set; }

    public SphereEventArgs(string name, Vector4 location, int colour)
    {
        this.Name = name;
        this.Location = location;
        this.Colour = colour;
    }
}