using Avalonia.Controls;
using PluginBrowser.ViewModels;

namespace PluginBrowser.Views;

/// <summary>
/// View for the Plugin Browser document tab.
/// Starts the NuGet version check when attached to the visual tree
/// and cancels it when detached.
/// </summary>
public partial class PluginBrowserView : UserControl
{
    private CancellationTokenSource? mCts;

    /// <summary>Initializes the view.</summary>
    public PluginBrowserView()
    {
        InitializeComponent();
    }

    /// <inheritdoc/>
    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs pArgs)
    {
        base.OnAttachedToVisualTree(pArgs);

        if (DataContext is PluginBrowserViewModel lVm)
        {
            mCts = new CancellationTokenSource();
            _ = lVm.LoadAvailableVersionsAsync(mCts.Token);
        }
    }

    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs pArgs)
    {
        base.OnDetachedFromVisualTree(pArgs);

        mCts?.Cancel();
        mCts?.Dispose();
        mCts = null;
    }
}
