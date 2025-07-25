using System.Numerics;

namespace PPather.Graph;

public sealed class SearchLocation
{
    public Vector4 Location { get; }

    public string Description { get; }

    public SearchLocation(float x, float y, float z, float mapId, string description)
    {
        Location = new Vector4(x, y, z, mapId);
        Description = description;
    }
}
