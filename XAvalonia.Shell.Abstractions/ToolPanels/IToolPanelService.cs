namespace XAvalonia.Shell.Abstractions.ToolPanels;

/// <summary>
/// Allows plugins to add and remove tool panels in the shell's docked panel areas
/// (left, right, bottom, top). Panels registered before the dock context is available
/// are queued and applied automatically once it becomes available.
/// </summary>
public interface IToolPanelService
{
    /// <summary>
    /// Registers a tool panel in the dock area indicated by
    /// <see cref="ToolPanelContribution.Alignment"/>.
    /// If the dock context is not yet available the panel is queued
    /// and added once it becomes available.
    /// </summary>
    /// <param name="pContribution">Description of the panel to add.</param>
    void RegisterPanel(ToolPanelContribution pContribution);

    /// <summary>
    /// Removes a previously registered panel from the dock.
    /// No-op if no panel with that identifier exists.
    /// </summary>
    /// <param name="pId">Identifier of the panel to remove.</param>
    void UnregisterPanel(string pId);
}
