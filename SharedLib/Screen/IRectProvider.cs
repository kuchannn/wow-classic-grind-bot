using SixLabors.ImageSharp;

namespace SharedLib;

public interface IRectProvider
{
    void GetPosition(ref Point point);
    void GetRectangle(out Rectangle rect);
}