namespace XAvalonia.Shell.Abstractions.Documents;

/// <summary>
/// Allows plugins to open, close, and activate documents in the shell's central document area.
/// Documents registered before the dock layout is ready are queued and applied automatically
/// once the dock context is connected.
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Registers and opens a document in the central document area.
    /// If the dock context is not yet available the document is queued
    /// and opened once it becomes available.
    /// </summary>
    /// <param name="pContribution">Description of the document to open.</param>
    void RegisterDocument(DocumentContribution pContribution);

    /// <summary>
    /// Closes the document with the given identifier.
    /// No-op if no document with that identifier is open.
    /// </summary>
    /// <param name="pId">Identifier of the document to close.</param>
    void CloseDocument(string pId);

    /// <summary>
    /// Brings the document with the given identifier into focus.
    /// No-op if no document with that identifier is open.
    /// </summary>
    /// <param name="pId">Identifier of the document to activate.</param>
    void ActivateDocument(string pId);

    /// <summary>
    /// Returns <see langword="true"/> if a document with the given identifier is currently open
    /// in the dock; <see langword="false"/> if it was never opened or has been closed.
    /// </summary>
    /// <param name="pId">Identifier of the document to check.</param>
    bool IsDocumentOpen(string pId);

    /// <summary>
    /// Raised when a document is closed by the user or programmatically.
    /// The event argument is the document identifier.
    /// </summary>
    event EventHandler<string>? DocumentClosed;
}
