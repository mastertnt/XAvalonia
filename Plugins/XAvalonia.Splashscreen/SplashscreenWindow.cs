using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace XAvalonia.Splashscreen;

/// <summary>
/// Borderless, centered window showing the configured image with a message line at the bottom.
/// Built in code so the plugin has no XAML to compile.
/// </summary>
internal sealed class SplashscreenWindow : Window
{
    private const double DefaultWidth  = 480;
    private const double DefaultHeight = 270;

    private readonly TextBlock mMessage;
    private readonly Border mMessageBar;

    /// <summary>Builds the window from the given options.</summary>
    /// <param name="pOptions">Image, size and colors to use.</param>
    public SplashscreenWindow(SplashscreenOptions pOptions)
    {
        SystemDecorations     = SystemDecorations.None;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        CanResize             = false;
        ShowInTaskbar         = false;
        Topmost               = true;
        Background            = ParseBrush(pOptions.Background, Brushes.Black);

        Grid lRoot = new Grid();

        Bitmap? lBitmap = LoadImage(pOptions.ImagePath);
        if (lBitmap is not null)
        {
            lRoot.Children.Add(new Image
            {
                Source  = lBitmap,
                Stretch = Stretch.Uniform
            });
        }

        mMessage = new TextBlock
        {
            Foreground   = ParseBrush(pOptions.Foreground, Brushes.White),
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        mMessageBar = new Border
        {
            Background          = new SolidColorBrush(Color.FromArgb(0x80, 0, 0, 0)),
            Padding             = new Thickness(12, 6),
            VerticalAlignment   = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsVisible           = false,
            Child               = mMessage
        };
        lRoot.Children.Add(mMessageBar);

        Content = lRoot;

        // Explicit size wins; otherwise use the image's natural size, or a default size without image.
        Width  = pOptions.Width  ?? lBitmap?.Size.Width  ?? DefaultWidth;
        Height = pOptions.Height ?? lBitmap?.Size.Height ?? DefaultHeight;
    }

    /// <summary>Displays <paramref name="pMessage"/> in the message bar. Must be called on the UI thread.</summary>
    /// <param name="pMessage">Text to display; empty hides the bar.</param>
    public void SetMessage(string pMessage)
    {
        mMessage.Text         = pMessage;
        mMessageBar.IsVisible = !string.IsNullOrEmpty(pMessage);
    }

    private static Bitmap? LoadImage(string? pPath)
    {
        if (string.IsNullOrWhiteSpace(pPath))
        {
            return null;
        }

        try
        {
            if (pPath.StartsWith("avares://", StringComparison.OrdinalIgnoreCase))
            {
                using Stream lStream = AssetLoader.Open(new Uri(pPath));
                return new Bitmap(lStream);
            }

            string lFullPath = Path.IsPathRooted(pPath) ? pPath : Path.Combine(AppContext.BaseDirectory, pPath);
            if (!File.Exists(lFullPath))
            {
                Trace.WriteLine($"[Splashscreen] Image not found: '{lFullPath}'.");
                return null;
            }

            return new Bitmap(lFullPath);
        }
        catch (Exception lEx)
        {
            Trace.WriteLine($"[Splashscreen] Failed to load image '{pPath}': {lEx.Message}");
            return null;
        }
    }

    private static IBrush ParseBrush(string pColor, IBrush pFallback)
    {
        return Color.TryParse(pColor, out Color lColor) ? new SolidColorBrush(lColor) : pFallback;
    }
}
