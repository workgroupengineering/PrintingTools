using System;

namespace PrintingTools.Core;

public static class PrintServiceRegistry
{
    private static readonly object Sync = new();
    private static PrintingToolsOptions _options = new();
    private static IPrintAdapterResolver? _resolver;
    private static IPrintManager? _manager;

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
            _options = options.Clone();
            _resolver = null;
            _manager = null;
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
