namespace Core.GOAP;

public sealed class RemoveClosestPoi : GoapEventArgs
{
    public string Name { get; }

    public RemoveClosestPoi(string name)
    {
        Name = name;
    }
}
