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

    /// <summary>Lowest severity shown when the Log panel opens. Populated from <c>user_configuration.json → LogViewer → minimumLevel</c>.</summary>
    [UserConfiguration("minimumLevel",
        DisplayName = "Default Log Level",
        Description = "Lowest severity shown by default in the Log panel; lower levels can still be shown with the panel's filters.",
        Order = 10)]
    public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

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
        lViewModel.ShowLevelsFrom(MinimumLevel);
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
                if (pArgs.Setting.PluginId != Id)
                {
                    return;
                }

                lViewModel.MaxEntries = MaxEntries;
                if (pArgs.Setting.Key == "minimumLevel")
                {
                    lViewModel.ShowLevelsFrom(MinimumLevel);
                }
            };
        }

        lLog.LogInfo($"[{Name}] Plugin loaded successfully.");
    }
}
