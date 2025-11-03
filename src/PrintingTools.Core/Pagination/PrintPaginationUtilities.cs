using System;
using System.Collections.Generic;
using Avalonia;

namespace PrintingTools.Core.Pagination;

public static class PrintPaginationUtilities
{
    public static IEnumerable<PrintPage> ExpandPage(PrintPage page)
    {
        ArgumentNullException.ThrowIfNull(page);

        var metrics = page.Metrics ?? PrintPageMetrics.Create(page.Visual, page.Settings);

        var scale = metrics.ContentScale <= 0 ? 1d : metrics.ContentScale;
        var availableWidth = metrics.ContentRect.Width / scale;
        var availableHeight = metrics.ContentRect.Height / scale;

        if (availableWidth <= 0 || availableHeight <= 0)
        {
            yield return EnsurePageHasMetrics(page, metrics);
            yield break;
        }

        var baseOffset = metrics.ContentOffset;
        var visualHeight = metrics.VisualBounds.Height;
        var remainingHeight = Math.Max(visualHeight - baseOffset.Y, 0);

        if (remainingHeight <= availableHeight + 0.5)
        {
            yield return EnsurePageHasMetrics(page, metrics);
            yield break;
        }

        var pageCount = Math.Max(1, (int)Math.Ceiling(remainingHeight / availableHeight));
        var maxOffset = Math.Max(visualHeight - availableHeight, 0);
        var basePageBreak = page.IsPageBreakAfter;

        for (var index = 0; index < pageCount; index++)
        {
            var offsetY = baseOffset.Y + index * availableHeight;
            offsetY = Math.Min(offsetY, maxOffset);

            var offset = new Point(baseOffset.X, offsetY);
            var sliceMetrics = index == 0 && offset.Equals(metrics.ContentOffset)
                ? metrics
                : metrics.WithContentOffset(offset);

            var isLast = index == pageCount - 1;
            yield return new PrintPage(
                page.Visual,
                page.Settings,
                isLast ? basePageBreak : false,
                sliceMetrics);
        }
    }

    private static PrintPage EnsurePageHasMetrics(PrintPage page, PrintPageMetrics metrics)
    {
        if (ReferenceEquals(metrics, page.Metrics))
        {
            return page;
        }

        return new PrintPage(page.Visual, page.Settings, page.IsPageBreakAfter, metrics);
    }
}
