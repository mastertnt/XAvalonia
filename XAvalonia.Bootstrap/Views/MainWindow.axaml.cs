using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace XAvalonia.Bootstrap.Views;

/// <summary>Shell main window — menu bar, dock area, status bar.</summary>
public partial class MainWindow : Window
{
    /// <summary>Initializes the window from its XAML.</summary>
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
