namespace SharedLib.NpcFinder;

public enum SearchMode
{
    Simple = 0,
    Fuzzy = 1
}

public static class SearchMode_Extension
{
    public static string ToStringF(this SearchMode value) => value switch
    {
        SearchMode.Simple => nameof(SearchMode.Simple),
        SearchMode.Fuzzy => nameof(SearchMode.Fuzzy),
        _ => nameof(SearchMode.Simple),
    };
}