namespace XAvalonia.Shell.Abstractions.ToolPanels;

/// <summary>
/// Describes a tool panel contributed by a plugin to one of the shell's docked panel areas.
/// The <see cref="ViewModel"/> must implement <c>Dock.Model.Core.IDockable</c> at runtime
/// (typically a subclass of <c>Dock.Model.ReactiveUI.Controls.Tool</c>).
/// The abstraction layer carries it as <see cref="object"/> to avoid a Dock dependency.
/// </summary>
public sealed class ToolPanelContribution
{
    /// <summary>
    /// Initializes a new tool panel contribution.
    /// </summary>
    /// <param name="pId">
    /// Unique identifier used to remove the panel later.
    /// Must match the <c>Id</c> property set on <paramref name="pViewModel"/>.
    /// </param>
    /// <param name="pTitle">Tab title shown in the panel dock.</param>
    /// <param name="pViewModel">
    /// The panel view model. Must implement <c>Dock.Model.Core.IDockable</c> at runtime,
    /// typically a subclass of <c>Dock.Model.ReactiveUI.Controls.Tool</c>.
    /// </param>
    /// <param name="pAlignment">Which side of the dock area this panel belongs to.</param>
    public ToolPanelContribution(
        string pId,
        string pTitle,
        object pViewModel,
        ToolPanelAlignment pAlignment)
    {
        Id        = pId;
        Title     = pTitle;
        ViewModel = pViewModel;
        Alignment = pAlignment;
    }

    /// <summary>Unique identifier for this panel.</summary>
    public string Id { get; }

    /// <summary>Tab title shown in the docked panel area.</summary>
    public string Title { get; }

    /// <summary>
    /// The panel view model. Must implement <c>Dock.Model.Core.IDockable</c> at runtime.
    /// </summary>
    public object ViewModel { get; }

    /// <summary>Which side of the dock area hosts this panel.</summary>
    public ToolPanelAlignment Alignment { get; }
}
