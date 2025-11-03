using System;
using Avalonia;
using PrintingTools.Core;
using PrintingTools.MacOS;

namespace PrintingTools;

/// <summary>
/// Extends the Avalonia <see cref="AppBuilder"/> with PrintingTools integration hooks.
/// </summary>
public static class PrintingToolsAppBuilderExtensions
{
    public static AppBuilder UsePrintingTools(this AppBuilder builder, Action<PrintingToolsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new PrintingToolsOptions();
        configure?.Invoke(options);
        var capturedOptions = options.Clone();

        var factory = new MacPrintAdapterFactory();

        return builder.AfterSetup(_ =>
        {
            PrintServiceRegistry.Configure(capturedOptions);
            if (capturedOptions.AdapterFactory is null && factory.IsSupported)
            {
                capturedOptions.AdapterFactory = () => factory.CreateAdapter()!;
            }
        });
    }

    public static IPrintManager GetPrintManager() => PrintServiceRegistry.EnsureManager();

    public static PrintingToolsOptions GetPrintingOptions() => PrintServiceRegistry.Options;
}
