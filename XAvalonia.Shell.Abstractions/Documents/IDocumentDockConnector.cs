namespace XAvalonia.Shell.Abstractions.Documents;

/// <summary>
/// Connects the document service to the Dock framework objects created by the layout plugin.
/// The parameters are typed as <see cref="object"/> so this abstraction has no dependency
/// on Dock.Avalonia — at runtime they must be a <c>Dock.Model.Core.IFactory</c>
/// and a <c>Dock.Model.Controls.IDocumentDock</c> respectively.
/// </summary>
public interface IDocumentDockConnector
{
    /// <summary>
    /// Supplies the Dock factory and document dock to the service.
    /// Any documents queued before this call are flushed immediately.
    /// </summary>
    /// <param name="pFactory">
    /// The active <c>Dock.Model.Core.IFactory</c> used to manipulate the dock layout.
    /// </param>
    /// <param name="pDocumentDock">
    /// The <c>Dock.Model.Controls.IDocumentDock</c> that hosts document tabs.
    /// </param>
    void ConnectDock(object pFactory, object pDocumentDock);
}
