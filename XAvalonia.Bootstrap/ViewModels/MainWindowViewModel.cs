using System.Collections.ObjectModel;
using Avalonia.Controls;
using Dock.Model.Controls;
using XAvalonia.Bootstrap.Services;
using XAvalonia.Bootstrap.ViewModels.Menu;
using XAvalonia.Bootstrap.ViewModels.StatusBar;
using XAvalonia.Shell.Abstractions.Plugins;

namespace XAvalonia.Bootstrap.ViewModels;

/// <summary>
/// View model for the shell's main window.
/// Exposes the menu bar, status bar items, and the dock layout.
/// Implements <see cref="IShellDockService"/> so plugins can inject their layout.
/// </summary>
public sealed class MainWindowViewModel : ViewModelBase, IShellDockService
{
    private string mWindowTitle = "AvaloniaShell";
    private WindowIcon? mWindowIcon;
    private IRootDock? mLayout;

    /// <summary>Main window title bound to the window's Title property.</summary>
    public string WindowTitle
    {
        get => mWindowTitle;
        internal set => SetProperty(ref mWindowTitle, value);
    }

    /// <summary>Application icon bound to the window's Icon property.</summary>
    public WindowIcon? WindowIcon
    {
        get => mWindowIcon;
        internal set => SetProperty(ref mWindowIcon, value);
    }

    /// <summary>The root dock layout injected by the active layout plugin.</summary>
    public IRootDock? Layout
    {
        get => mLayout;
        private set => SetProperty(ref mLayout, value);
    }

    /// <summary>Initializes the view model from the shell services.</summary>
    public MainWindowViewModel(MenuService pMenuService, StatusBarService pStatusBarService)
    {
        Menus            = pMenuService.Menus;
        LeftStatusItems  = pStatusBarService.LeftItems;
        RightStatusItems = pStatusBarService.RightItems;
    }

    /// <summary>Top-level menus bound to the menu bar.</summary>
    public ReadOnlyObservableCollection<MenuViewModel> Menus { get; }

    /// <summary>Status bar items displayed on the left side.</summary>
    public ReadOnlyObservableCollection<StatusBarItemViewModel> LeftStatusItems { get; }

    /// <summary>Status bar items displayed on the right side.</summary>
    public ReadOnlyObservableCollection<StatusBarItemViewModel> RightStatusItems { get; }

    /// <inheritdoc/>
    public void SetLayout(object pRootDock)
    {
        Layout = (IRootDock)pRootDock;
    }
}
