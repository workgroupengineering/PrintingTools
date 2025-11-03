using System;
using System.Collections.Generic;
using System.Threading;
using PrintingTools.Core;

namespace PrintingTools.Core.Pagination;

/// <summary>
/// Provides a centralized place to translate a <see cref="PrintDocument"/> into a paginated set of <see cref="PrintPage"/>s.
/// </summary>
internal interface IPrintPaginator
{
    IReadOnlyList<PrintPage> Paginate(PrintDocument document, PrintOptions options, CancellationToken cancellationToken = default);
}

internal sealed class DefaultPrintPaginator : IPrintPaginator
{
    public IReadOnlyList<PrintPage> Paginate(PrintDocument document, PrintOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(options);

        var pages = new List<PrintPage>();

        using var enumerator = document.CreateEnumerator();
        while (enumerator.MoveNext(cancellationToken))
        {
            var page = enumerator.Current;
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var expanded in PrintPaginationUtilities.ExpandPage(page))
            {
                pages.Add(expanded);
            }
        }

        return pages;
    }
}
