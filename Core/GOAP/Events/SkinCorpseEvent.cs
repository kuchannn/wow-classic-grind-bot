using System;
using System.Numerics;

namespace Core.GOAP;

public sealed class SkinCorpseEvent : GoapEventArgs
{
    public const string NAME = "Skin";
    public const string COLOR = "white";

    public Vector3 MapLoc { get; }
    public float Radius { get; }
    public int NpcId { get; }

    public SkinCorpseEvent(Vector3 location, float radius, int npcId)
    {
        MapLoc = location;
        Radius = MathF.Max(1, radius);
        NpcId = npcId;
    }
}
