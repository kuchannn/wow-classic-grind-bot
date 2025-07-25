using SharedLib.Extensions;

using SixLabors.ImageSharp;

namespace SharedLib.NpcFinder;

public readonly record struct NpcPosition
{
    private static readonly NpcPosition empty = new(Point.Empty, Point.Empty, 0, 0);
    public static ref readonly NpcPosition Empty => ref empty;

    public readonly Rectangle Rect;
    public readonly Point ClickPoint;

    public NpcPosition(Point min, Point max, int yOffset, float heightMul)
        : this(new(min.X, min.Y, max.X - min.X, max.Y - min.Y), yOffset, heightMul)
    { }

    public NpcPosition(Rectangle rect, int yOffset, float heightMul)
    {
        Rect = rect;
        ClickPoint = Rect.BottomCentre();
        ClickPoint.Offset(0, yOffset + (int)(Rect.Height * heightMul));
    }
}