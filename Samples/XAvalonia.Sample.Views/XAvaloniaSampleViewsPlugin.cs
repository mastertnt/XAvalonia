using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Shell.Abstractions.Documents;
using XAvalonia.Shell.Abstractions.Icons;
using XAvalonia.Shell.Abstractions.Logging;
using XAvalonia.Shell.Abstractions.Plugins;
using XAvalonia.Shell.Abstractions.Selection;
using XAvalonia.Shell.Abstractions.Shell;
using XAvalonia.Shell.Abstractions.Splashscreen;
using XAvalonia.Shell.Abstractions.StatusBar;
using XAvalonia.Shell.Abstractions.ToolPanels;

namespace XAvalonia.Sample.Views;

/// <summary>
/// Plugin that contributes the Explorer, Properties, and Output tool panels
/// to the shell's docked panel areas via <see cref="IToolPanelService"/>.
/// </summary>
[DependsOnService(typeof(ILogService))]
public sealed class XAvaloniaSampleViewsPlugin : IPlugin
{
    private const string SelectionStatusItemId = "sample.selection";
    private const string EditorCategory = "Text Editor";

    private readonly EditorSettings mEditorSettings = new EditorSettings();

    /// <summary>Font size of the sample documents. Populated from <c>user_configuration.json → XAvaloniaSampleViews → editor.fontSize</c>.</summary>
    [UserConfiguration("editor.fontSize", DisplayName = "Font Size", Category = EditorCategory + "/Font", Order = 10,
        Description = "Controls the font size in pixels of the sample documents.", Minimum = 6, Maximum = 72)]
    public double EditorFontSize { get; set; } = 13;

    /// <summary>Font family of the sample documents.</summary>
    [UserConfiguration("editor.fontFamily", DisplayName = "Font Family", Category = EditorCategory + "/Font", Order = 20,
        Description = "Controls the font family of the sample documents; separate fallbacks with commas.")]
    public string EditorFontFamily { get; set; } = "Consolas,Menlo,monospace";

    /// <summary>How long lines wrap in the sample documents.</summary>
    [UserConfiguration("editor.wordWrap", DisplayName = "Word Wrap", Category = EditorCategory, Order = 30,
        Description = "Controls how long lines wrap in the sample documents.")]
    public TextWrapping EditorWordWrap { get; set; } = TextWrapping.Wrap;

    /// <summary>Whether Tab inserts a tab character in the sample documents.</summary>
    [UserConfiguration("editor.acceptsTab", DisplayName = "Accepts Tab", Category = EditorCategory, Order = 40,
        Description = "Insert a tab character when pressing Tab instead of moving the focus to the next control.")]
    public bool EditorAcceptsTab { get; set; } = true;

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

        ApplyEditorSettings();
        IUserConfiguration? lUserConfiguration = pServiceManager.TryRequestService<IUserConfiguration>();
        if (lUserConfiguration is not null)
        {
            lUserConfiguration.SettingChanged += (_, pArgs) =>
            {
                if (pArgs.Setting.PluginId == Id)
                {
                    ApplyEditorSettings();
                }
            };
        }

        lDocumentService.RegisterDocument(new DocumentContribution(
            pId: "doc1",
            pTitle: "Document 1",
            pViewModel: new DocumentViewModel("Document 1", "Contenu du premier document.\n\nModifiez-moi !", mEditorSettings)));

        lDocumentService.RegisterDocument(new DocumentContribution(
            pId: "doc2",
            pTitle: "Document 2",
            pViewModel: new DocumentViewModel("Document 2", "Contenu du second document.", mEditorSettings)));

        lLog?.LogDebug($"[{Name}] Requesting IToolPanelService…");
        IToolPanelService lToolPanelService = pServiceManager.RequestService<IToolPanelService>();
        lLog?.LogDebug($"[{Name}] IToolPanelService acquired.");

        IIconManager lIconManager = pServiceManager.RequestService<IIconManager>();
        ISelectionManager lSelectionManager = pServiceManager.RequestService<ISelectionManager>();

        lSplash?.ShowMessage("Registering sample tool panels…");

        lToolPanelService.RegisterPanel(new ToolPanelContribution(
            pId: "explorer",
            pTitle: "Explorateur",
            pViewModel: new ExplorerViewModel(lIconManager, lSelectionManager),
            pAlignment: ToolPanelAlignment.Left));
        lLog?.LogDebug($"[{Name}] Registered tool panel 'explorer'.");

        lToolPanelService.RegisterPanel(new ToolPanelContribution(
            pId: "properties",
            pTitle: "Propriétés",
            pViewModel: new PropertiesViewModel(lSelectionManager),
            pAlignment: ToolPanelAlignment.Right));
        lLog?.LogDebug($"[{Name}] Registered tool panel 'properties'.");

        lToolPanelService.RegisterPanel(new ToolPanelContribution(
            pId: "output",
            pTitle: "Sortie",
            pViewModel: new OutputViewModel(),
            pAlignment: ToolPanelAlignment.Bottom));
        lLog?.LogDebug($"[{Name}] Registered tool panel 'output'.");

        // Selection example: the active selection (e.g. the explorer node) is shown in the status bar.
        IStatusBarService lStatusBar = pServiceManager.RequestService<IStatusBarService>();
        lStatusBar.RegisterItem(new StatusBarItemContribution(SelectionStatusItemId, FormatSelection(null), StatusBarItemAlignment.Left, 300));
        lSelectionManager.ActiveSelectionChanged += (_, pArgs) =>
            lStatusBar.UpdateItem(SelectionStatusItemId, FormatSelection(pArgs.Context.PrimaryItem));
        lLog?.LogDebug($"[{Name}] Registered status bar item '{SelectionStatusItemId}'.");

        lLog?.LogInfo($"[{Name}] Plugin loaded successfully.");
    }

    // Copies the [UserConfiguration] properties into the settings observed by the documents.
    private void ApplyEditorSettings()
    {
        mEditorSettings.FontSize   = EditorFontSize;
        mEditorSettings.WordWrap   = EditorWordWrap;
        mEditorSettings.AcceptsTab = EditorAcceptsTab;
        mEditorSettings.FontFamily = string.IsNullOrWhiteSpace(EditorFontFamily)
            ? FontFamily.Default
            : FontFamily.Parse(EditorFontFamily);
    }

    private static string FormatSelection(object? pItem)
        => pItem is FileNode lNode ? $"Sélection : {lNode.Name}" : "Sélection : (aucune)";
}
