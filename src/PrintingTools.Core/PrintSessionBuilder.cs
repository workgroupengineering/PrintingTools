using System;
using System.Collections.Generic;
using Avalonia;

namespace PrintingTools.Core;

public sealed class PrintSessionBuilder
{
    private readonly List<Func<IPrintPageEnumerator>> _pageSources = new();
    private readonly PrintOptions _options = new();

    public PrintSessionBuilder AddVisual(Visual visual, PrintPageSettings? settings = null)
    {
        ArgumentNullException.ThrowIfNull(visual);
        if (!PrintLayoutHints.GetIsPrintable(visual))
        {
            return this;
        }

        var baseSettings = (settings ?? PrintPageSettings.Default).Clone();
        var pageSettings = PrintLayoutHints.CreateSettings(visual, baseSettings);

        _pageSources.Add(() => new VisualPrintPageEnumerator(visual, pageSettings, PrintLayoutHints.GetIsPageBreakAfter(visual)));
        return this;
    }

    public PrintSessionBuilder AddPageSource(Func<IPrintPageEnumerator> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _pageSources.Add(factory);
        return this;
    }

    public PrintSessionBuilder ConfigureOptions(Action<PrintOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        configure(_options);
        return this;
    }

    public PrintSession Build(string? description = null)
    {
        if (_pageSources.Count == 0)
        {
            throw new InvalidOperationException("At least one page source must be added to build a session.");
        }

        var factories = _pageSources.ToArray();
        var document = PrintDocument.FromFactories(factories);
        return new PrintSession(document, _options.Clone(), description);
    }
}
