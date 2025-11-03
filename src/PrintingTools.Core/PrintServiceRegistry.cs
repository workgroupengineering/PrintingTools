using System;

namespace PrintingTools.Core;

public static class PrintServiceRegistry
{
    private static readonly object Sync = new();
    private static PrintingToolsOptions _options = new();
    private static IPrintAdapterResolver? _resolver;
    private static IPrintManager? _manager;
    private static Action<PrintDiagnosticEvent>? _diagnosticSubscription;

    public static PrintingToolsOptions Options
    {
        get
        {
            lock (Sync)
            {
                return _options;
            }
        }
    }

    public static void Configure(PrintingToolsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        lock (Sync)
        {
            if (_diagnosticSubscription is not null)
            {
                PrintDiagnostics.UnregisterSink(_diagnosticSubscription);
                _diagnosticSubscription = null;
            }

            _options = options.Clone();
            _resolver = null;
            _manager = null;

            if (_options.DiagnosticSink is { } sink)
            {
                PrintDiagnostics.RegisterSink(sink);
                _diagnosticSubscription = sink;
            }
        }
    }

    public static IPrintAdapterResolver EnsureResolver()
    {
        lock (Sync)
        {
            _resolver ??= new DefaultPrintAdapterResolver(_options);
            return _resolver;
        }
    }

    public static IPrintManager EnsureManager()
    {
        lock (Sync)
        {
            _manager ??= new PrintManager(EnsureResolver(), _options);
            return _manager;
        }
    }
}
