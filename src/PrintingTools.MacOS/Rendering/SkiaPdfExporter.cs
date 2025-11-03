using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Skia.Helpers;
using PrintingTools.Core;
using SkiaSharp;

namespace PrintingTools.MacOS.Rendering;

internal static class SkiaPdfExporter
{
    private const double DipsPerInch = 96d;
    private const double PointsPerInch = 72d;
    private const string DiagnosticsCategory = "SkiaPdfExporter";

    public static void Export(string path, IReadOnlyList<PrintPage> pages)
    {
        if (pages.Count == 0)
        {
            return;
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        PrintDiagnostics.Report(
            DiagnosticsCategory,
            $"Exporting managed PDF to '{path}'.",
            context: new { PageCount = pages.Count });

        using var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        using var document = SKDocument.CreatePdf(stream) ?? throw new InvalidOperationException("Unable to create PDF document via Skia.");

        foreach (var page in pages)
        {
            RenderPage(document, page);
        }

        document.Close();
    }

    public static byte[] CreatePdfBytes(IReadOnlyList<PrintPage> pages)
    {
        if (pages.Count == 0)
        {
            return Array.Empty<byte>();
        }

        using var wStream = new SKDynamicMemoryWStream();
        using (var document = SKDocument.CreatePdf(wStream) ?? throw new InvalidOperationException("Unable to create PDF document via Skia."))
        {
            for (var i = 0; i < pages.Count; i++)
            {
                var tag = (pages[i].Visual as Control)?.Tag;
                PrintDiagnostics.Report(
                    DiagnosticsCategory,
                    $"Rendering PDF page {i}",
                    context: new { Index = i, Tag = tag });
                RenderPage(document, pages[i]);
            }

            document.Close();
        }

        using var data = wStream.DetachAsData();
        return data?.ToArray() ?? Array.Empty<byte>();
    }

    private static void RenderPage(SKDocument document, PrintPage page)
    {
        var metrics = page.Metrics ?? PrintPageMetrics.Create(page.Visual, page.Settings);

        var width = (float)(metrics.PageSize.Width * PointsPerInch / DipsPerInch);
        var height = (float)(metrics.PageSize.Height * PointsPerInch / DipsPerInch);

        using var canvas = document.BeginPage(width, height);
        canvas.Clear(SKColors.White);

        var dipsToPoints = (float)(PointsPerInch / DipsPerInch);
        canvas.Scale(dipsToPoints);
        canvas.Translate((float)metrics.ContentRect.X, (float)metrics.ContentRect.Y);
        canvas.Scale((float)metrics.ContentScale);
        canvas.Translate((float)(-metrics.ContentOffset.X - metrics.VisualBounds.X), (float)(-metrics.ContentOffset.Y - metrics.VisualBounds.Y));

        DrawingContextHelper.RenderAsync(canvas, page.Visual, page.Visual.Bounds, metrics.Dpi).GetAwaiter().GetResult();

        document.EndPage();
    }
}
