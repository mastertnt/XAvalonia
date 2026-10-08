namespace XAvalonia.Shell.Abstractions.Documents;

/// <summary>
/// Describes a document tab contributed by a plugin to the central document area.
/// The <see cref="ViewModel"/> must implement <c>Dock.Model.Core.IDockable</c> at runtime,
/// but the abstraction layer carries it as <see cref="object"/> to avoid a Dock dependency.
/// </summary>
public sealed class DocumentContribution
{
    /// <summary>
    /// Initializes a new document contribution.
    /// </summary>
    /// <param name="pId">
    /// Unique identifier used to activate or close the document later.
    /// Must match the <c>Id</c> property of <paramref name="pViewModel"/>.
    /// </param>
    /// <param name="pTitle">Tab title displayed in the document area.</param>
    /// <param name="pViewModel">
    /// The document view model. Must implement <c>Dock.Model.Core.IDockable</c>
    /// (typically a subclass of <c>Dock.Model.ReactiveUI.Controls.Document</c>).
    /// </param>
    public DocumentContribution(string pId, string pTitle, object pViewModel)
    {
        Id        = pId;
        Title     = pTitle;
        ViewModel = pViewModel;
    }

    /// <summary>Unique identifier for this document.</summary>
    public string Id { get; }

    /// <summary>Tab title shown in the document area.</summary>
    public string Title { get; }

    /// <summary>
    /// The document view model. Must implement <c>Dock.Model.Core.IDockable</c> at runtime.
    /// </summary>
    public object ViewModel { get; }
}
