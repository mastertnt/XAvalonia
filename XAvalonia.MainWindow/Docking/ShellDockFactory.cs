using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI;
using Dock.Model.ReactiveUI.Controls;

namespace Avalonia.MainWindow.Docking;

/// <summary>
/// Builds the Dock layout: left panel (Explorer), central area (Documents),
/// right panel (Properties) and bottom panel (Output).
/// </summary>
public class ShellDockFactory : Factory
{
    private IRootDock? mRootDock;
    private IDocumentDock? mDocumentDock;
    private ToolDock? mLeftDock;
    private ToolDock? mRightDock;
    private ToolDock? mBottomDock;

    /// <summary>The document dock created by <see cref="CreateLayout"/>. Available after layout creation.</summary>
    public IDocumentDock? DocumentDock => mDocumentDock;

    /// <summary>The left-side tool dock. Available after layout creation.</summary>
    public ToolDock? LeftDock => mLeftDock;

    /// <summary>The right-side tool dock. Available after layout creation.</summary>
    public ToolDock? RightDock => mRightDock;

    /// <summary>The bottom tool dock. Available after layout creation.</summary>
    public ToolDock? BottomDock => mBottomDock;

    public override IRootDock CreateLayout()
    {
        // DocumentDock starts empty; documents are contributed by plugins via IDocumentService.
        mDocumentDock = new DocumentDock
        {
            Id = "DocumentsDock",
            Title = "Documents",
            IsCollapsable = false,
            VisibleDockables = CreateList<IDockable>(),
            CanCreateDocument = true,
            DockCapabilityPolicy = new DockCapabilityPolicy(),
            DockCapabilityOverrides = new DockCapabilityOverrides()
        };

        // Tool panes — start empty; panels are contributed by plugins via IToolPanelService.
        mLeftDock = new ToolDock
        {
            Id = "LeftPane",
            Title = "LeftPane",
            Proportion = 0.20,
            VisibleDockables = CreateList<IDockable>(),
            Alignment = Alignment.Left,
            DockCapabilityPolicy = new DockCapabilityPolicy(),
            DockCapabilityOverrides = new DockCapabilityOverrides()
        };

        mRightDock = new ToolDock
        {
            Id = "RightPane",
            Title = "RightPane",
            Proportion = 0.20,
            VisibleDockables = CreateList<IDockable>(),
            Alignment = Alignment.Right,
            DockCapabilityPolicy = new DockCapabilityPolicy(),
            DockCapabilityOverrides = new DockCapabilityOverrides()
        };

        mBottomDock = new ToolDock
        {
            Id = "BottomPane",
            Title = "BottomPane",
            Proportion = 0.22,
            VisibleDockables = CreateList<IDockable>(),
            Alignment = Alignment.Bottom,
            DockCapabilityPolicy = new DockCapabilityPolicy(),
            DockCapabilityOverrides = new DockCapabilityOverrides()
        };

        // Centre column: documents stacked above the bottom pane
        ProportionalDock lCentreColumn = new ProportionalDock
        {
            Id = "CentreColumn",
            Orientation = Orientation.Vertical,
            DockCapabilityPolicy = new DockCapabilityPolicy(),
            DockCapabilityOverrides = new DockCapabilityOverrides(),
            VisibleDockables = CreateList<IDockable>(
                mDocumentDock,
                new ProportionalDockSplitter(),
                mBottomDock
            )
        };

        // Main row: left | centre | right
        ProportionalDock lMainRow = new ProportionalDock
        {
            Id = "MainRow",
            Orientation = Orientation.Horizontal,
            DockCapabilityPolicy = new DockCapabilityPolicy(),
            DockCapabilityOverrides = new DockCapabilityOverrides(),
            VisibleDockables = CreateList<IDockable>(
                mLeftDock,
                new ProportionalDockSplitter(),
                lCentreColumn,
                new ProportionalDockSplitter(),
                mRightDock
            )
        };

        // Root dock
        RootDock lRootDock = new RootDock
        {
            Id = "Root",
            IsCollapsable = false,
            DockCapabilityPolicy = new DockCapabilityPolicy(),
            DockCapabilityOverrides = new DockCapabilityOverrides(),
            RootDockCapabilityPolicy = new DockCapabilityPolicy(),
            VisibleDockables = CreateList<IDockable>(lMainRow),
            DefaultDockable = lMainRow
        };

        mRootDock = lRootDock;
        return lRootDock;
    }

    public override void InitLayout(IDockable pLayout)
    {
        HostWindowLocator = new Dictionary<string, Func<IHostWindow?>>
        {
            [nameof(IDockWindow)] = () => new HostWindow()
        };

        DockableLocator = new Dictionary<string, Func<IDockable?>>
        {
            ["Root"]          = () => mRootDock,
            ["DocumentsDock"] = () => mDocumentDock,
            ["LeftPane"]      = () => mLeftDock,
            ["RightPane"]     = () => mRightDock,
            ["BottomPane"]    = () => mBottomDock,
        };

        base.InitLayout(pLayout);
    }
}
