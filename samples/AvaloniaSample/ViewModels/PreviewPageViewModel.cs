using Avalonia.Media.Imaging;

namespace AvaloniaSample.ViewModels;

public sealed class PreviewPageViewModel
{
    public PreviewPageViewModel(int number, RenderTargetBitmap image)
    {
        Number = number;
        Image = image;
    }

    public int Number { get; }

    public RenderTargetBitmap Image { get; }
}
