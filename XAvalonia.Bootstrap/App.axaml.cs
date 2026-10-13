using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Bootstrap.Plugins;
using XAvalonia.Bootstrap.Services;
using XAvalonia.Bootstrap.ViewModels;
using XAvalonia.Shell.Abstractions.Shell;

namespace XAvalonia.Bootstrap;

/// <summary>Avalonia application that initializes the plugins and shows the shell main window.</summary>
public partial class App : Application
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>Initializes the plugins, then creates the main window and attaches its lifecycle events.</summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lDesktop)
        {
            // Phase 2 — initialize plugins with shell services.
            // Must run after Avalonia is initialized so plugins can safely interact with the UI.
            ShellApplication.Services.GetRequiredService<PluginManager>()
                .InitializeAll(ShellApplication.Services);

            IMainWindowService lMainWindowService =
                ShellApplication.Services.GetRequiredService<IMainWindowService>();

            MainWindowViewModel lVm = ShellApplication.Services.GetRequiredService<MainWindowViewModel>();
            Views.MainWindow lWindow = new Views.MainWindow { DataContext = lVm };
            lDesktop.MainWindow = lWindow;

            // Wire up window lifecycle events (Loaded / AboutToQuit / Closed).
            ShellApplication.Services.GetRequiredService<MainWindowService>().AttachWindow(lWindow);

            
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void LWindow_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
    {
        throw new NotImplementedException();
    }
}
