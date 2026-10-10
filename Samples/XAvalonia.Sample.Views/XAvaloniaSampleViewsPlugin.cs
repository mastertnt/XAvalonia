using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Shell.Abstractions.Documents;
using XAvalonia.Shell.Abstractions.Icons;
using XAvalonia.Shell.Abstractions.Logging;
using XAvalonia.Shell.Abstractions.Plugins;
using XAvalonia.Shell.Abstractions.Splashscreen;
using XAvalonia.Shell.Abstractions.ToolPanels;

namespace XAvalonia.Sample.Views;

/// <summary>
/// Plugin that contributes the Explorer, Properties, and Output tool panels
/// to the shell's docked panel areas via <see cref="IToolPanelService"/>.
/// </summary>
public sealed class XAvaloniaSampleViewsPlugin : IPlugin
{
    /// <inheritdoc/>
    public string Id => "org.avaloniaui.shell.sample-views";

    /// <inheritdoc/>
    public string Name => "Sample Views";

    /// <inheritdoc/>
    public string Description => "Contributes the Explorer (left), Properties (right), and Output (bottom) tool panels.";

    /// <inheritdoc/>
    public Version CurrentVersion => new Version(1, 0, 0);

    /// <inheritdoc/>
    public string? NuGetPackageId => null;

    /// <inheritdoc/>
    public IReadOnlyList<Type> ProvidedServices => Array.Empty<Type>();

    /// <inheritdoc/>
    public void RegisterServices(IServiceCollection pServices) { }

    /// <inheritdoc/>
    public void Initialize(IPluginServiceManager pServiceManager)
    {
        ILogService? lLog = pServiceManager.TryRequestService<ILogService>();
        lLog?.LogInfo($"[{Name}] Loading plugin…");

        ISplashscreen? lSplash = pServiceManager.TryRequestService<ISplashscreen>();
        lSplash?.ShowMessage("Opening sample documents…");

        lLog?.LogDebug($"[{Name}] Requesting IDocumentService…");
        IDocumentService lDocumentService = pServiceManager.RequestService<IDocumentService>();
        lLog?.LogDebug($"[{Name}] IDocumentService acquired.");

        lDocumentService.RegisterDocument(new DocumentContribution(
            pId: "doc1",
            pTitle: "Document 1",
            pViewModel: new DocumentViewModel("Document 1", "Contenu du premier document.\n\nModifiez-moi !")));

        lDocumentService.RegisterDocument(new DocumentContribution(
            pId: "doc2",
            pTitle: "Document 2",
            pViewModel: new DocumentViewModel("Document 2", "Contenu du second document.")));

        lLog?.LogDebug($"[{Name}] Requesting IToolPanelService…");
        IToolPanelService lToolPanelService = pServiceManager.RequestService<IToolPanelService>();
        lLog?.LogDebug($"[{Name}] IToolPanelService acquired.");

        IIconManager lIconManager = pServiceManager.RequestService<IIconManager>();

        lSplash?.ShowMessage("Registering sample tool panels…");

        lToolPanelService.RegisterPanel(new ToolPanelContribution(
            pId: "explorer",
            pTitle: "Explorateur",
            pViewModel: new ExplorerViewModel(lIconManager),
            pAlignment: ToolPanelAlignment.Left));
        lLog?.LogDebug($"[{Name}] Registered tool panel 'explorer'.");

        lToolPanelService.RegisterPanel(new ToolPanelContribution(
            pId: "properties",
            pTitle: "Propriétés",
            pViewModel: new PropertiesViewModel(),
            pAlignment: ToolPanelAlignment.Right));
        lLog?.LogDebug($"[{Name}] Registered tool panel 'properties'.");

        lToolPanelService.RegisterPanel(new ToolPanelContribution(
            pId: "output",
            pTitle: "Sortie",
            pViewModel: new OutputViewModel(),
            pAlignment: ToolPanelAlignment.Bottom));
        lLog?.LogDebug($"[{Name}] Registered tool panel 'output'.");

        lLog?.LogInfo($"[{Name}] Plugin loaded successfully.");
    }
}
