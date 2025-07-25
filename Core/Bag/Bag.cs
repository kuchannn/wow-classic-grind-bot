using SharedLib;

namespace Core;

public sealed class Bag
{
    public Item Item { get; set; }
    public BagType BagType { get; set; }
    public int SlotCount { get; set; }
    public int FreeSlot { get; set; }
}
