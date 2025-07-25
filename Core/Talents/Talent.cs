namespace Core.Talents;

public struct Talent
{
    public int Hash { get; init; }
    public int TabNum { get; init; }
    public int TierNum { get; init; }
    public int ColumnNum { get; init; }
    public int CurrentRank { get; init; }
    public string Name { get; set; }

    public override string ToString()
    {
        return $"{TabNum} - {TierNum} - {ColumnNum} - {CurrentRank} - {Name}";
    }
}
