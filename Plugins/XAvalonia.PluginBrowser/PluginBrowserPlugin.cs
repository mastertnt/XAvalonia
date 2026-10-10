using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PluginBrowser.ViewModels;
using XAvalonia.Shell.Abstractions.Documents;
using XAvalonia.Shell.Abstractions.Logging;
using XAvalonia.Shell.Abstractions.Menus;
using XAvalonia.Shell.Abstractions.Plugins;

namespace PluginBrowser;

/// <summary>
/// Plugin that adds a "Plugins" top-level menu with a "Browse Plugins…" command.
/// Opens the Plugin Browser as a docked document tab in the central area.
/// Discovered automatically by <c>PluginLoader</c> at shell startup.
/// </summary>
[DependsOnService(typeof(ILogService))]
public sealed class PluginBrowserPlugin : IPlugin
{
    private IPluginManager? mPluginManager;
    private IDocumentService? mDocumentService;
    private ILogService? mLog;
    private PluginBrowserViewModel? mViewModel;

    /// <inheritdoc/>
    public string Id => "org.avaloniaui.shell.plugin-browser";

    /// <inheritdoc/>
    public string Name => "Plugin Browser";

    /// <inheritdoc/>
    public string Description => "Displays all installed plugins, their versions, and checks NuGet for updates.";

    /// <inheritdoc/>
    public Version CurrentVersion => new Version(1, 0, 0);

    /// <inheritdoc/>
    /// <remarks>Built-in plugin; no NuGet package to check.</remarks>
    public string? NuGetPackageId => null;

    /// <inheritdoc/>
    public IReadOnlyList<Type> ProvidedServices => Array.Empty<Type>();

    /// <inheritdoc/>
    /// <remarks>This plugin registers no additional services.</remarks>
    public void RegisterServices(IServiceCollection pServices)
    {
    }

    /// <inheritdoc/>
    public void Initialize(IPluginServiceManager pServiceManager)
    {
        ILogService? lLog = pServiceManager.TryRequestService<ILogService>();
        mLog = lLog;
        lLog?.LogInfo($"[{Name}] Loading plugin…");

        lLog?.LogDebug($"[{Name}] Requesting IPluginManager…");
        mPluginManager = pServiceManager.RequestService<IPluginManager>();
        lLog?.LogDebug($"[{Name}] IPluginManager acquired.");

        lLog?.LogDebug($"[{Name}] Requesting IDocumentService…");
        mDocumentService = pServiceManager.RequestService<IDocumentService>();
        lLog?.LogDebug($"[{Name}] IDocumentService acquired.");

        lLog?.LogDebug($"[{Name}] Requesting IMenuService…");
        IMenuService lMenuService = pServiceManager.RequestService<IMenuService>();
        lLog?.LogDebug($"[{Name}] IMenuService acquired.");

        lMenuService.RegisterMenu(new MenuContribution("plugins", "_Plugins", 800));
        lLog?.LogDebug($"[{Name}] Registered menu 'plugins'.");

        lMenuService.RegisterMenuItem("plugins", new MenuItemContribution(
            pId: "plugins.browse",
            pHeader: "_Browse Plugins…",
            pCommand: new RelayCommand(OpenBrowser),
            pOrder: 100));
        lLog?.LogDebug($"[{Name}] Registered menu item 'plugins.browse'.");

        mDocumentService.DocumentClosed += OnDocumentClosed;
        lLog?.LogDebug($"[{Name}] Subscribed to DocumentClosed.");

        lLog?.LogInfo($"[{Name}] Plugin loaded successfully.");
    }

    private void OnDocumentClosed(object? pSender, string pId)
    {
        mLog?.LogInfo($"[{Name}] DocumentClosed fired: id='{pId}'");
        if (pId == "plugin-browser")
        {
            mViewModel = null;
        }
    }

    // Opens the Plugin Browser document tab, or re-activates it if already open.
    private void OpenBrowser()
    {
        bool lIsOpen = mDocumentService!.IsDocumentOpen("plugin-browser");
        mLog?.LogInfo($"[{Name}] OpenBrowser: IsDocumentOpen={lIsOpen}");

        if (lIsOpen)
        {
            mDocumentService.ActivateDocument("plugin-browser");
            return;
        }

        mLog?.LogInfo($"[{Name}] OpenBrowser: creating and registering new ViewModel.");
        mViewModel = new PluginBrowserViewModel(mPluginManager!);
        try
        {
            mDocumentService.RegisterDocument(new DocumentContribution(
                pId: "plugin-browser",
                pTitle: "Plugin Browser",
                pViewModel: mViewModel));
            mLog?.LogInfo($"[{Name}] OpenBrowser: RegisterDocument done.");
        }
        catch (Exception lEx)
        {
            mLog?.LogError($"[{Name}] OpenBrowser: RegisterDocument threw: {lEx}");
            throw;
        }
    }
}
