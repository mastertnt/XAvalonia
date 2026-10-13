using Avalonia.Media;
using ReactiveUI;

namespace XAvalonia.Sample.Views;

/// <summary>
/// Text editor settings shared by every sample document.
/// Updated by <see cref="XAvaloniaSampleViewsPlugin"/> when the user configuration changes.
/// </summary>
public sealed class EditorSettings : ReactiveObject
{
    private double mFontSize = 13;
    private FontFamily mFontFamily = FontFamily.Parse("Consolas,Menlo,monospace");
    private TextWrapping mWordWrap = TextWrapping.Wrap;
    private bool mAcceptsTab = true;

    /// <summary>Font size of the documents, in pixels.</summary>
    public double FontSize
    {
        get => mFontSize;
        set => this.RaiseAndSetIfChanged(ref mFontSize, value);
    }

    /// <summary>Font family of the documents.</summary>
    public FontFamily FontFamily
    {
        get => mFontFamily;
        set => this.RaiseAndSetIfChanged(ref mFontFamily, value);
    }

    /// <summary>How long lines wrap.</summary>
    public TextWrapping WordWrap
    {
        get => mWordWrap;
        set => this.RaiseAndSetIfChanged(ref mWordWrap, value);
    }

    /// <summary>Whether Tab inserts a tab character instead of moving the focus.</summary>
    public bool AcceptsTab
    {
        get => mAcceptsTab;
        set => this.RaiseAndSetIfChanged(ref mAcceptsTab, value);
    }
}
