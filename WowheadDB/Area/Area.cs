using System.Collections.Generic;

namespace WowheadDB;

public sealed class Area
{
    public List<NPC> flightmaster;
    public List<NPC> innkeeper;
    public List<NPC> repair;
    public List<NPC> vendor;
    public List<NPC> trainer;
    public Dictionary<string, List<Node>> herb;
    public Dictionary<string, List<Node>> vein;

    public int[] skinnable;
    public int[] gatherable;
    public int[] minable;
    public int[] salvegable;
}