namespace Core;

public sealed class NoBlacklist : IBlacklist
{
    public bool Is() => false;
}