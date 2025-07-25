using System.Drawing;

namespace SharedLib.Extensions;

public static class RectangleExt
{
    public static Point Centre(this Rectangle r)
    {
        return new Point(r.Left + r.Width / 2, r.Top + r.Height / 2);
    }

    public static Point Max(this Rectangle r)
    {
        return new Point(r.Left + r.Width, r.Top + r.Height);
    }

    public static Point BottomCentre(this Rectangle r)
    {
        return new Point(r.Left + r.Width / 2, r.Bottom);
    }
}
