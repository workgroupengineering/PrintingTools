namespace PrintingTools.Core;

/// <summary>
/// Provides a simple diagnostics pipeline so callers can observe printing events without
/// taking direct dependencies on Console or Debug output.
/// </summary>
public static class PrintDiagnostics
{
    private static readonly object Sync = new();
    private static Action<PrintDiagnosticEvent>? _subscribers;

    public static void RegisterSink(Action<PrintDiagnosticEvent> sink)
    {
        ArgumentNullException.ThrowIfNull(sink);

        lock (Sync)
        {
            _subscribers += sink;
        }
    }

    public static void UnregisterSink(Action<PrintDiagnosticEvent> sink)
    {
        ArgumentNullException.ThrowIfNull(sink);

        lock (Sync)
        {
            _subscribers -= sink;
        }
    }

    public static void Report(string category, string message, Exception? exception = null, object? context = null)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            throw new ArgumentException("Category must be supplied.", nameof(category));
        }

        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        Action<PrintDiagnosticEvent>? subscribers;
        lock (Sync)
        {
            subscribers = _subscribers;
        }

        subscribers?.Invoke(new PrintDiagnosticEvent(DateTimeOffset.UtcNow, category, message, exception, context));
    }
}

public sealed record PrintDiagnosticEvent(
    DateTimeOffset Timestamp,
    string Category,
    string Message,
    Exception? Exception,
    object? Context);
