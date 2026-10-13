using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Shell.Abstractions.Logging;
using XAvalonia.Shell.Abstractions.Plugins;
using XAvalonia.Shell.Abstractions.Shell;
using XAvalonia.Shell.Abstractions.ToolPanels;

namespace XAvalonia.LogViewer;

/// <summary>
/// Plugin that registers <see cref="ILogService"/> and contributes the Log Viewer tool panel.
/// </summary>
public sealed class LogViewerPlugin : IPlugin
{
    /// <summary>Number of messages kept in the Log panel. Populated from <c>user_configuration.json → LogViewer → maxEntries</c>.</summary>
    [UserConfiguration("maxEntries",
        DisplayName = "Maximum Entries",
        Description = "Number of messages kept in the Log panel; the oldest ones are discarded first.",
        Minimum = 100, Maximum = 100000)]
    public int MaxEntries { get; set; } = 5000;

    /// <inheritdoc/>
    public string Id => "org.avaloniaui.shell.log-viewer";

    /// <inheritdoc/>
    public string Name => "Log Viewer";

    /// <inheritdoc/>
    public string Description => "Displays shell and plugin log messages with per-severity filtering.";

    /// <inheritdoc/>
    public Version CurrentVersion => new Version(1, 0, 0);

    /// <inheritdoc/>
    public string? NuGetPackageId => null;

    /// <inheritdoc/>
    public IReadOnlyList<Type> ProvidedServices => new[] { typeof(ILogService) };

    /// <inheritdoc/>
    public void RegisterServices(IServiceCollection pServices)
    {
        pServices.AddSingleton<ILogService, LogService>();
    }

    /// <inheritdoc/>
    public void Initialize(IPluginServiceManager pServiceManager)
    {
        ILogService lLog = pServiceManager.RequestService<ILogService>();
        lLog.LogInfo($"[{Name}] Loading plugin…");

        lLog.LogDebug($"[{Name}] Requesting IToolPanelService…");
        IToolPanelService lToolPanelService = pServiceManager.RequestService<IToolPanelService>();
        lLog.LogDebug($"[{Name}] IToolPanelService acquired.");

        LogViewerViewModel lViewModel = new LogViewerViewModel(lLog) { MaxEntries = MaxEntries };
        lToolPanelService.RegisterPanel(new ToolPanelContribution(
            pId: "log-viewer",
            pTitle: "Log",
            pViewModel: lViewModel,
            pAlignment: ToolPanelAlignment.Bottom));
        lLog.LogDebug($"[{Name}] Registered tool panel 'log-viewer'.");

        // Apply settings edited at runtime (e.g. from the settings editor).
        IUserConfiguration? lUserConfiguration = pServiceManager.TryRequestService<IUserConfiguration>();
        if (lUserConfiguration is not null)
        {
            lUserConfiguration.SettingChanged += (_, pArgs) =>
            {
                if (pArgs.Setting.PluginId == Id)
                {
                    lViewModel.MaxEntries = MaxEntries;
                }
            };
        }

        lLog.LogInfo($"[{Name}] Plugin loaded successfully.");
    }
}
