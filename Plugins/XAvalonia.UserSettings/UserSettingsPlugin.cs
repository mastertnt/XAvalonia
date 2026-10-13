using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Shell.Abstractions.Documents;
using XAvalonia.Shell.Abstractions.Logging;
using XAvalonia.Shell.Abstractions.Menus;
using XAvalonia.Shell.Abstractions.Plugins;
using XAvalonia.Shell.Abstractions.Shell;
using XAvalonia.UserSettings.ViewModels;

namespace XAvalonia.UserSettings;

/// <summary>
/// Plugin that adds a "Settings…" command (Ctrl+,) to the File menu.
/// Opens a settings editor document listing every <c>[UserConfiguration]</c> property declared by the loaded plugins,
/// grouped by category, searchable, and editable; changes are saved to <c>user_configuration.json</c> immediately.
/// </summary>
[DependsOnService(typeof(ILogService))]
[DependsOnPlugin("org.avaloniaui.shell.mainwindow", Optional = true)]
public sealed class UserSettingsPlugin : IPlugin
{
    private const string DocumentId = "user-settings";

    private IUserConfiguration? mUserConfiguration;
    private IDocumentService? mDocumentService;
    private ILogService? mLog;
    private UserSettingsViewModel? mViewModel;

    /// <inheritdoc/>
    public string Id => "org.avaloniaui.shell.user-settings";

    /// <inheritdoc/>
    public string Name => "Settings Editor";

    /// <inheritdoc/>
    public string Description => "Browse, search and edit the user settings declared by plugins.";

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
        mLog = pServiceManager.TryRequestService<ILogService>();
        mLog?.LogInfo($"[{Name}] Loading plugin…");

        mUserConfiguration = pServiceManager.RequestService<IUserConfiguration>();
        mDocumentService   = pServiceManager.RequestService<IDocumentService>();
        IMenuService lMenuService = pServiceManager.RequestService<IMenuService>();

        RegisterMenuItems(lMenuService);
        mDocumentService.DocumentClosed += OnDocumentClosed;

        mLog?.LogInfo($"[{Name}] Plugin loaded successfully.");
    }

    // Adds "Settings…" to the File menu, or to a "Tools" menu when no File menu exists.
    private void RegisterMenuItems(IMenuService pMenuService)
    {
        MenuItemContribution lItem = new MenuItemContribution(
            pId: "file.settings",
            pHeader: "_Settings…",
            pCommand: new RelayCommand(OpenSettings),
            pOrder: 420,
            pInputGesture: "Ctrl+OemComma");

        try
        {
            pMenuService.RegisterMenuItem("file", lItem);
            pMenuService.RegisterMenuItem("file", new MenuSeparatorContribution("file.sep.settings", 450));
        }
        catch (InvalidOperationException)
        {
            mLog?.LogDebug($"[{Name}] No 'file' menu; registering a 'tools' menu instead.");
            pMenuService.RegisterMenu(new MenuContribution("tools", "_Tools", 700));
            pMenuService.RegisterMenuItem("tools", lItem);
        }
    }

    private void OnDocumentClosed(object? pSender, string pId)
    {
        if (pId == DocumentId)
        {
            mViewModel?.Detach();
            mViewModel = null;
        }
    }

    // Opens the settings editor document tab, or re-activates it if already open.
    private void OpenSettings()
    {
        if (mDocumentService!.IsDocumentOpen(DocumentId))
        {
            mDocumentService.ActivateDocument(DocumentId);
            return;
        }

        mViewModel?.Detach();
        mViewModel = new UserSettingsViewModel(mUserConfiguration!);
        mDocumentService.RegisterDocument(new DocumentContribution(
            pId: DocumentId,
            pTitle: "Settings",
            pViewModel: mViewModel));
        mLog?.LogInfo($"[{Name}] Opened settings editor ({mViewModel.TotalCount} settings).");
    }
}
