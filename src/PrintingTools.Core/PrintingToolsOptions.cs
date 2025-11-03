using System;

namespace PrintingTools.Core;

public sealed class PrintingToolsOptions
{
    public bool EnablePreview { get; set; } = true;

    public Func<IPrintAdapter>? AdapterFactory { get; set; }

    public Action<PrintDiagnosticEvent>? DiagnosticSink { get; set; }

    public PrintingToolsOptions Clone() =>
        new()
        {
            EnablePreview = EnablePreview,
            AdapterFactory = AdapterFactory,
            DiagnosticSink = DiagnosticSink
        };
}
