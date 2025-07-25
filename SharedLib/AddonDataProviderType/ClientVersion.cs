namespace SharedLib;

public enum ClientVersion
{
    None,
    Retail,
    SoM,
    TBC,
    Wrath,
    Cata
}

public static class ClientVersion_Extension
{
    public static string ToStringF(this ClientVersion value) => value switch
    {
        ClientVersion.None => nameof(ClientVersion.None),
        ClientVersion.Retail => nameof(ClientVersion.Retail),
        ClientVersion.SoM => nameof(ClientVersion.SoM),
        ClientVersion.TBC => nameof(ClientVersion.TBC),
        ClientVersion.Wrath => nameof(ClientVersion.Wrath),
        ClientVersion.Cata => nameof(ClientVersion.Cata),
        _ => nameof(ClientVersion.None)
    };
}
