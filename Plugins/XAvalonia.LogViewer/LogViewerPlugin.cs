using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Shell.Abstractions.Logging;
using XAvalonia.Shell.Abstractions.Plugins;
using XAvalonia.Shell.Abstractions.ToolPanels;

namespace XAvalonia.LogViewer;

/// <summary>
/// Plugin that registers <see cref="ILogService"/> and contributes the Log Viewer tool panel.
/// </summary>
public sealed class LogViewerPlugin : IPlugin
{
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

        lToolPanelService.RegisterPanel(new ToolPanelContribution(
            pId: "log-viewer",
            pTitle: "Log",
            pViewModel: new LogViewerViewModel(lLog),
            pAlignment: ToolPanelAlignment.Bottom));
        lLog.LogDebug($"[{Name}] Registered tool panel 'log-viewer'.");

        lLog.LogInfo($"[{Name}] Plugin loaded successfully.");
    }
}
