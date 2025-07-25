namespace Core;

public enum Gossip
{
    Banker,
    Battlemaster,
    Binder,
    Gossip,
    Healer,
    Petition,
    Tabard,
    Taxi,
    Trainer,
    Unlearn,
    Vendor
}

public static class Gossip_Extension
{
    public static string ToStringF(this Gossip value) => value switch
    {
        Gossip.Banker => nameof(Gossip.Banker),
        Gossip.Battlemaster => nameof(Gossip.Battlemaster),
        Gossip.Binder => nameof(Gossip.Binder),
        Gossip.Gossip => nameof(Gossip.Gossip),
        Gossip.Healer => nameof(Gossip.Healer),
        Gossip.Petition => nameof(Gossip.Petition),
        Gossip.Tabard => nameof(Gossip.Tabard),
        Gossip.Taxi => nameof(Gossip.Taxi),
        Gossip.Trainer => nameof(Gossip.Trainer),
        Gossip.Unlearn => nameof(Gossip.Unlearn),
        Gossip.Vendor => nameof(Gossip.Vendor),
        _ => throw new System.NotImplementedException()
    };
}
