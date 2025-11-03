using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using PrintingTools.Core;
using PrintingTools.MacOS;
using AvaloniaSample.ViewModels;

namespace AvaloniaSample;

public partial class MainWindow : Window
{
    private readonly MacPrintAdapterFactory _adapterFactory = new();
    private PrintPreviewModel? _currentPreview;
    private bool _isRendering;

    public MainWindow()
    {
        InitializeComponent();

        if (_adapterFactory.IsSupported)
        {
            StatusText.Text = "Ready to render sample pages.";
        }
        else
        {
            StatusText.Text = "macOS printing adapter unavailable on this platform.";
            PreviewButton.IsEnabled = false;
            NativePreviewButton.IsEnabled = false;
            ExportPdfButton.IsEnabled = false;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        DisposePreview();
    }

    private async void OnPreviewClicked(object? sender, RoutedEventArgs e)
    {
        if (_isRendering)
        {
            return;
        }

        if (!_adapterFactory.IsSupported)
        {
            StatusText.Text = "macOS adapter is unavailable.";
            return;
        }

        _isRendering = true;
        PreviewButton.IsEnabled = false;
        ExportPdfButton.IsEnabled = false;
        StatusText.Text = "Generating preview…";

        try
        {
            DisposePreview();

            var adapter = _adapterFactory.CreateAdapter();
            if (adapter is null)
            {
                StatusText.Text = "macOS adapter could not be created.";
                return;
            }

            var session = BuildSampleSession();
            var preview = await adapter.CreatePreviewAsync(session);
            _currentPreview = preview;

            var viewModel = new PrintPreviewViewModel(preview.Pages, preview.Images);
            var window = new PrintPreviewWindow(viewModel);
            await window.ShowDialog(this);
            StatusText.Text = "Preview closed.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Preview failed: {ex.Message}";
        }
        finally
        {
            _isRendering = false;
            PreviewButton.IsEnabled = _adapterFactory.IsSupported;
            NativePreviewButton.IsEnabled = _adapterFactory.IsSupported;
            ExportPdfButton.IsEnabled = _adapterFactory.IsSupported;
        }
    }

    private async void OnNativePreviewClicked(object? sender, RoutedEventArgs e)
    {
        if (_isRendering)
        {
            return;
        }

        if (!_adapterFactory.IsSupported)
        {
            StatusText.Text = "macOS adapter is unavailable.";
            return;
        }

        _isRendering = true;
        PreviewButton.IsEnabled = false;
        NativePreviewButton.IsEnabled = false;
        ExportPdfButton.IsEnabled = false;
        StatusText.Text = "Opening macOS print preview…";

        try
        {
            DisposePreview();

            var adapter = _adapterFactory.CreateAdapter();
            if (adapter is null)
            {
                StatusText.Text = "macOS adapter could not be created.";
                return;
            }

            var session = BuildSampleSession();
            session.Options.ShowPrintDialog = true;
            session.Options.UseManagedPdfExporter = true;
            session.Options.PdfOutputPath = null;

            await adapter.PrintAsync(session);

            StatusText.Text = "macOS preview closed.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"macOS preview failed: {ex.Message}";
        }
        finally
        {
            _isRendering = false;
            PreviewButton.IsEnabled = _adapterFactory.IsSupported;
            NativePreviewButton.IsEnabled = _adapterFactory.IsSupported;
            ExportPdfButton.IsEnabled = _adapterFactory.IsSupported;
        }
    }

    private async void OnExportPdfClicked(object? sender, RoutedEventArgs e)
    {
        if (_isRendering)
        {
            return;
        }

        if (!_adapterFactory.IsSupported)
        {
            StatusText.Text = "macOS adapter is unavailable.";
            return;
        }

        _isRendering = true;
        PreviewButton.IsEnabled = false;
        ExportPdfButton.IsEnabled = false;
        StatusText.Text = "Exporting PDF…";

        try
        {
            DisposePreview();

            var adapter = _adapterFactory.CreateAdapter();
            if (adapter is null)
            {
                StatusText.Text = "macOS adapter could not be created.";
                return;
            }

            var session = BuildSampleSession();
            var pdfPath = GetPdfDestination();
            session.Options.PdfOutputPath = pdfPath;
            session.Options.ShowPrintDialog = false;
            session.Options.UseManagedPdfExporter = true;

            await adapter.PrintAsync(session);

            StatusText.Text = $"PDF exported to {pdfPath}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"PDF export failed: {ex.Message}";
        }
        finally
        {
            PreviewButton.IsEnabled = _adapterFactory.IsSupported;
            NativePreviewButton.IsEnabled = _adapterFactory.IsSupported;
            ExportPdfButton.IsEnabled = _adapterFactory.IsSupported;
            _isRendering = false;
        }
    }

    private PrintSession BuildSampleSession()
    {
        var builder = new PrintSessionBuilder();

        for (var page = 1; page <= 4; page++)
        {
            var visual = CreateSamplePage(page);
            builder.AddVisual(visual);
        }

        builder.ConfigureOptions(options =>
        {
            options.ShowPrintDialog = false;
            options.CollectPreviewFirst = true;
        });

        return builder.Build("Quarterly Report");
    }

    private Control CreateSamplePage(int pageNumber)
    {
        const double PageWidth = 816;
        const double PageHeight = 1056;
        const double PageMargin = 48;
        var contentWidth = PageWidth - (PageMargin * 2);
        var contentHeight = PageHeight - (PageMargin * 2);

        var accent = pageNumber % 2 == 0 ? Colors.SteelBlue : Colors.DarkOrange;
        var accentBrush = new SolidColorBrush(accent);

        var content = new Border
        {
            Width = contentWidth,
            Height = contentHeight,
            Background = Brushes.White,
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(24),
            Tag = $"SamplePage-{pageNumber}",
            Child = new StackPanel
            {
                Spacing = 24,
                Children =
                {
                    new TextBlock
                    {
                        Text = $"Quarterly Report – Page {pageNumber}",
                        FontSize = 30,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = accentBrush
                    },
                    new TextBlock
                    {
                        Text = "This preview demonstrates the macOS printing pipeline rendering Avalonia visuals " +
                               "into Quartz contexts. The layout mirrors a simple report page with typography, " +
                               "summary metrics, and highlights.",
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 16
                    },
                    CreateMetricsGrid(accentBrush),
                    CreateHighlightsSection(pageNumber)
                }
            }
        };

        PrintLayoutHints.SetTargetPageSize(content, new Size(PageWidth, PageHeight));
        PrintLayoutHints.SetMargins(content, new Thickness(PageMargin));
        PrintLayoutHints.SetScale(content, 1d);

        PrepareVisual(content);
        return content;
    }

    private static Control CreateMetricsGrid(SolidColorBrush accentBrush)
    {
        var labels = new[] { "Revenue", "Expenses", "Delta" };
        var values = new[] { "$128K", "$86K", "$42K" };

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*"),
            RowDefinitions = new RowDefinitions("Auto,Auto"),
            ColumnSpacing = 12,
            RowSpacing = 6
        };

        for (var i = 0; i < labels.Length; i++)
        {
            var label = new TextBlock
            {
                Text = labels[i],
                FontSize = 14,
                FontWeight = FontWeight.SemiBold,
                Foreground = accentBrush
            };

            Grid.SetColumn(label, i);
            Grid.SetRow(label, 0);
            grid.Children.Add(label);

            var value = new TextBlock
            {
                Text = values[i],
                FontSize = 22,
                FontWeight = FontWeight.Bold
            };

            Grid.SetColumn(value, i);
            Grid.SetRow(value, 1);
            grid.Children.Add(value);
        }

        var accentColor = accentBrush.Color;

        return new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(20, accentColor.R, accentColor.G, accentColor.B)),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(16),
            Child = grid
        };
    }

    private static Control CreateHighlightsSection(int pageNumber)
    {
        var highlights = new List<string>
        {
            "Feature parity milestones advanced for the macOS backend.",
            "Quartz rendering validated against Avalonia visuals.",
            $"Preview pipeline now returns raster snapshots (page {pageNumber}).",
            "Next step: translate PrintOptions into NSPrintInfo settings."
        };

        var stack = new StackPanel
        {
            Spacing = 6
        };

        stack.Children.Add(new TextBlock
        {
            Text = "Highlights",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold
        });

        foreach (var highlight in highlights)
        {
            stack.Children.Add(new TextBlock
            {
                Text = $"• {highlight}",
                TextWrapping = TextWrapping.Wrap,
                FontSize = 15
            });
        }

        return stack;
    }

    private static void PrepareVisual(Control control)
    {
        if (control is not Layoutable layoutable)
        {
            return;
        }

        var width = !double.IsNaN(control.Width) && control.Width > 0 ? control.Width : 816;
        var height = !double.IsNaN(control.Height) && control.Height > 0 ? control.Height : 1056;
        var size = new Size(width, height);

        layoutable.Measure(size);
        layoutable.Arrange(new Rect(size));
    }

    private void DisposePreview()
    {
        if (_currentPreview is null)
        {
            return;
        }

        _currentPreview.Dispose();
        _currentPreview = null;
    }

    private static string GetPdfDestination()
    {
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var fallback = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var baseDirectory = string.IsNullOrWhiteSpace(desktop) ? fallback : desktop;
        var fileName = $"AvaloniaSample-Print-{DateTime.Now:yyyyMMdd-HHmmss}.pdf";
        return Path.Combine(baseDirectory, fileName);
    }
}
