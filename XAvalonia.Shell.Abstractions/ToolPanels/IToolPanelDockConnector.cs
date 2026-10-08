namespace XAvalonia.Shell.Abstractions.ToolPanels;

/// <summary>
/// Connects the tool panel service to the Dock objects created by the layout plugin.
/// All parameters are typed as <see cref="object"/> so this abstraction has no
/// dependency on Dock.Avalonia. At runtime they must be:
/// <list type="bullet">
///   <item><paramref name="pFactory"/> — a <c>Dock.Model.Core.IFactory</c></item>
///   <item>dock parameters — <c>Dock.Model.Core.IDock</c> instances (may be null if a zone is absent)</item>
/// </list>
/// </summary>
public interface IToolPanelDockConnector
{
    /// <summary>
    /// Supplies the Dock factory and the zone docks to the service.
    /// Any panels queued before this call are flushed immediately.
    /// Calling this a second time (e.g. on layout reset) re-adds all registered panels
    /// to the freshly created docks.
    /// </summary>
    /// <param name="pFactory">The active <c>Dock.Model.Core.IFactory</c>.</param>
    /// <param name="pLeftDock">Left-side <c>IDock</c>, or <c>null</c> if absent.</param>
    /// <param name="pRightDock">Right-side <c>IDock</c>, or <c>null</c> if absent.</param>
    /// <param name="pBottomDock">Bottom <c>IDock</c>, or <c>null</c> if absent.</param>
    void ConnectDocks(object pFactory, object? pLeftDock, object? pRightDock, object? pBottomDock);
}
