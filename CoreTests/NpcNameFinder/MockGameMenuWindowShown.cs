using Core;

namespace CoreTests;

internal sealed class MockGameMenuWindowShown : IGameMenuWindowShown
{
    public bool GameMenuWindowShown() => false;
}
