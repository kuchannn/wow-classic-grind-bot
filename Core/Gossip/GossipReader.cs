using System.Collections.Generic;

namespace Core;

public sealed class GossipReader : IReader
{
    private const int cGossip = 73;

    public int Count { private set; get; }
    public Dictionary<Gossip, int> Gossips { get; } = new();

    private int data;

    public bool Ready => Gossips.Count == Count;

    public bool GossipStart() => data == 69;
    public bool GossipEnd() => data == 9999994;

    public bool MerchantWindowOpened() => data == 9999999;

    public bool MerchantWindowClosed() => data == 9999998;

    public bool MerchantWindowSelling() => data == 9999997;

    public bool MerchantWindowSellingFinished() => data == 9999996;

    public bool GossipStartOrMerchantWindowOpened() => GossipStart() || MerchantWindowOpened();

    public GossipReader()
    {
    }

    public void Update(IAddonDataProvider reader)
    {
        data = reader.GetInt(cGossip);

        // used for merchant window open state
        if (MerchantWindowClosed() ||
            MerchantWindowOpened() ||
            MerchantWindowSelling() ||
            MerchantWindowSellingFinished() ||
            GossipEnd())
            return;

        if (data == 0 || GossipStart())
        {
            Count = 0;
            Gossips.Clear();
            return;
        }

        // formula
        // 10000 * count + 100 * index + value
        Count = data / 10000;
        int order = data / 100 % 100;
        Gossip gossip = (Gossip)(data % 100);

        Gossips[gossip] = order;
    }
}
