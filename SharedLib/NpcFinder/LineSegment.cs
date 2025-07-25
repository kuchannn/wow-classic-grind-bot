namespace SharedLib.NpcFinder;

public readonly record struct LineSegment
{
    private readonly int x;
    public readonly int Y;

    public readonly int XStart => x & 0xFFFF;
    public readonly int XEnd => x >> 16;
    public readonly int XCenter => XStart + ((XEnd - XStart) / 2);

    public LineSegment(int xStart, int xEnd, int y)
    {
        x = (xEnd << 16) | (xStart & 0xFFFF);
        Y = y;
    }
}