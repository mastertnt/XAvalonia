using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace XAvalonia.Sample.Views;

/// <summary>Generic document displayed in the central tab area.</summary>
public sealed class DocumentViewModel : Document
{
    private string mContent = string.Empty;

    /// <summary>Initializes a document with its title and initial content.</summary>
    /// <param name="pTitle">Tab title.</param>
    /// <param name="pContent">Initial text content.</param>
    public DocumentViewModel(string pTitle, string pContent)
    {
        Title   = pTitle;
        Id      = pTitle;
        Content = pContent;
        DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanPin = true,
            CanFloat = true,
            CanClose = true
        };
    }

    /// <summary>Editable text content bound to the document view.</summary>
    public string Content
    {
        get => mContent;
        set => this.RaiseAndSetIfChanged(ref mContent, value);
    }
}
