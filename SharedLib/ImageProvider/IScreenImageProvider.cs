using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SharedLib;

public interface IScreenImageProvider
{
    Image<Bgra32> ScreenImage { get; }

    Rectangle ScreenRect { get; }
}
