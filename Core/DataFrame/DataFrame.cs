namespace Core;

public readonly struct DataFrame
{
    public readonly int Index;
    public readonly int X;
    public readonly int Y;

    public DataFrame(int index, int x, int y)
    {
        Index = index;
        X = x;
        Y = y;
    }
}