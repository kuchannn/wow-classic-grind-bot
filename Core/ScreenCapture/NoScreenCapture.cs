using Microsoft.Extensions.Logging;

namespace Core;

public sealed class NoScreenCapture : ScreenCaptureCleaner
{
    public NoScreenCapture(ILogger logger, DataConfig dataConfig)
        : base(logger, dataConfig) { }

    public override void Request() { }
}