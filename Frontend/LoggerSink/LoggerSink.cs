using Serilog.Core;
using Serilog.Events;

namespace Frontend;

public sealed class LoggerSink : ILogEventSink
{
    public event Action? OnLogChanged;

    public const int SIZE = 256;
    private const int MOD = SIZE - 1;

    private int callCount;
    public LogEvent[] Log { get; }
    public int Head => callCount & MOD;

    public LoggerSink()
    {
        Log = new LogEvent[SIZE];
    }

    public void Emit(LogEvent logEvent)
    {
        Log[callCount++ & MOD] = logEvent;
        OnLogChanged?.Invoke();
    }
}
