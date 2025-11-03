using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;

namespace PrintingTools.Core;

public sealed class PrintPreviewModel : IDisposable
{
    private bool _disposed;

    public PrintPreviewModel(IReadOnlyList<PrintPage> pages, IReadOnlyList<RenderTargetBitmap> images)
    {
        Pages = pages ?? throw new ArgumentNullException(nameof(pages));
        Images = images ?? throw new ArgumentNullException(nameof(images));

        if (Pages.Count != Images.Count)
        {
            throw new ArgumentException("The number of preview images must match the number of pages.", nameof(images));
        }
    }

    public IReadOnlyList<PrintPage> Pages { get; }

    public IReadOnlyList<RenderTargetBitmap> Images { get; }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        foreach (var image in Images)
        {
            image?.Dispose();
        }

        _disposed = true;
    }
}
