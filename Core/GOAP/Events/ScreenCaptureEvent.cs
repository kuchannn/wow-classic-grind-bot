namespace Core.GOAP;

public sealed class ScreenCaptureEvent : GoapEventArgs
{
    public static readonly ScreenCaptureEvent Default = new();

    private ScreenCaptureEvent() { }
}
