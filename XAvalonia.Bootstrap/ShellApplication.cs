using System.Text.Json;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Bootstrap.Plugins;
using XAvalonia.Bootstrap.Services;
using XAvalonia.Bootstrap.ViewModels;
using XAvalonia.Shell.Abstractions.Documents;
using XAvalonia.Shell.Abstractions.Icons;
using XAvalonia.Shell.Abstractions.Menus;
using XAvalonia.Shell.Abstractions.Plugins;
using XAvalonia.Shell.Abstractions.Selection;
using XAvalonia.Shell.Abstractions.Shell;
using XAvalonia.Shell.Abstractions.StatusBar;
using XAvalonia.Shell.Abstractions.ToolPanels;

namespace XAvalonia.Bootstrap;

/// <summary>
/// Entry point helper for a shell application built on XAvalonia.Bootstrap.
/// Encapsulates DI registration, plugin discovery, and the Avalonia AppBuilder.
/// </summary>
public static class ShellApplication
{
    /// <summary>The application-wide service provider, available after <see cref="Run"/> is called.</summary>
    public static IServiceProvider Services { get; private set; } = null!;

    /// <summary>
    /// Builds services, then starts the Avalonia desktop application.
    /// This method does not return until the main window is closed.
    /// </summary>
    /// <param name="pArgs">Command-line arguments forwarded to Avalonia.</param>
    public static void Run(string[] pArgs)
    {
        Services = BuildServices();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(pArgs);
    }

    /// <summary>
    /// Configures the Avalonia <see cref="AppBuilder"/>.
    /// Can be called directly for IDE previewer support.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    // Registers all core shell services and runs plugin Phase 1 (RegisterServices).
    private static IServiceProvider BuildServices()
    {
        ServiceCollection lServices = new ServiceCollection();

        // IconManager: aggregates IIconProvider instances registered by plugins.
        lServices.AddSingleton<IconManager>();
        lServices.AddSingleton<IIconManager>(pSp => pSp.GetRequiredService<IconManager>());

        // MenuService: same singleton exposed under both concrete and interface keys.
        lServices.AddSingleton<MenuService>();
        lServices.AddSingleton<IMenuService>(pSp => pSp.GetRequiredService<MenuService>());

        // StatusBarService: same singleton exposed under both concrete and interface keys.
        lServices.AddSingleton<StatusBarService>();
        lServices.AddSingleton<IStatusBarService>(pSp => pSp.GetRequiredService<StatusBarService>());

        // SelectionManager: global and local selection contexts shared between plugins.
        lServices.AddSingleton<SelectionManager>();
        lServices.AddSingleton<ISelectionManager>(pSp => pSp.GetRequiredService<SelectionManager>());

        // TechnicalConfigurationService: exposes technical_configuration.json sections to plugins.
        JsonDocument lTechDocument = ReadJsonFile("technical_configuration.json");
        string lRawPluginsDir = lTechDocument.RootElement.TryGetProperty("pluginsDirectory", out JsonElement lDirEl)
            ? lDirEl.GetString() ?? "Plugins"
            : "Plugins";
        TechnicalConfigurationService lTechConfig = new TechnicalConfigurationService(lTechDocument);
        lServices.AddSingleton(lTechConfig);
        lServices.AddSingleton<ITechnicalConfiguration>(pSp => pSp.GetRequiredService<TechnicalConfigurationService>());

        // UserConfigurationService: exposes user_configuration.json sections to plugins.
        UserConfigurationService lUserConfig = new UserConfigurationService(ReadJsonFile("user_configuration.json"));
        lServices.AddSingleton(lUserConfig);
        lServices.AddSingleton<IUserConfiguration>(pSp => pSp.GetRequiredService<UserConfigurationService>());

        // Discover plugins and let them register their own services (Phase 1).
        string lPluginsDir = Path.IsPathRooted(lRawPluginsDir)
            ? lRawPluginsDir
            : Path.Combine(AppContext.BaseDirectory, lRawPluginsDir);
        PluginManager lPluginManager = new PluginManager(PluginLoader.LoadFromDirectory(lPluginsDir));
        lPluginManager.RegisterAllServices(lServices);

        lServices.AddSingleton(lPluginManager);
        lServices.AddSingleton<IPluginManager>(pSp => pSp.GetRequiredService<PluginManager>());

        // ToolPanelService: manages tool panels (left/right/bottom) in the Dock layout.
        lServices.AddSingleton<ToolPanelService>();
        lServices.AddSingleton<IToolPanelService>(pSp => pSp.GetRequiredService<ToolPanelService>());
        lServices.AddSingleton<IToolPanelDockConnector>(pSp => pSp.GetRequiredService<ToolPanelService>());

        // DocumentService: manages documents in the Dock layout.
        lServices.AddSingleton<DocumentService>();
        lServices.AddSingleton<IDocumentService>(pSp => pSp.GetRequiredService<DocumentService>());
        lServices.AddSingleton<IDocumentDockConnector>(pSp => pSp.GetRequiredService<DocumentService>());

        // MainWindowService: exposes title, icon, and shutdown to plugins.
        lServices.AddSingleton<MainWindowService>();
        lServices.AddSingleton<IMainWindowService>(pSp => pSp.GetRequiredService<MainWindowService>());

        // MainWindowViewModel also implements IShellDockService so plugins can inject a layout.
        lServices.AddSingleton<MainWindowViewModel>();
        lServices.AddSingleton<IShellDockService>(pSp => pSp.GetRequiredService<MainWindowViewModel>());

        return lServices.BuildServiceProvider();
    }

    private static JsonDocument ReadJsonFile(string pFileName)
    {
        string lPath = Path.Combine(AppContext.BaseDirectory, pFileName);
        return File.Exists(lPath)
            ? JsonDocument.Parse(File.ReadAllText(lPath))
            : JsonDocument.Parse("{}");
    }

}
