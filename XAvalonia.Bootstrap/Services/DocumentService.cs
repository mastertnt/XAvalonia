using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using XAvalonia.Shell.Abstractions.Documents;

namespace XAvalonia.Bootstrap.Services;

/// <summary>
/// <see cref="IDocumentService"/> implementation that manages documents in the Dock layout.
/// Registered as singleton under <c>DocumentService</c>, <c>IDocumentService</c>,
/// and <c>IDocumentDockConnector</c>.
/// Documents registered before <see cref="ConnectDock"/> is called are queued and
/// flushed automatically once the dock context is available.
/// </summary>
public sealed class DocumentService : IDocumentService, IDocumentDockConnector
{
    private readonly List<DocumentContribution> mPendingDocuments = new List<DocumentContribution>();
    private IFactory? mFactory;
    private IDocumentDock? mDocumentDock;

    /// <inheritdoc/>
    public event EventHandler<string>? DocumentClosed;

    /// <inheritdoc/>
    public void ConnectDock(object pFactory, object pDocumentDock)
    {
        if (mFactory is not null)
        {
            mFactory.DockableClosed -= OnFactoryDockableClosed;
        }

        mFactory     = (IFactory)pFactory;
        mDocumentDock = (IDocumentDock)pDocumentDock;

        mFactory.DockableClosed += OnFactoryDockableClosed;

        foreach (DocumentContribution lPending in mPendingDocuments)
        {
            AddToDock(lPending);
        }

        mPendingDocuments.Clear();
    }

    /// <inheritdoc/>
    public void RegisterDocument(DocumentContribution pContribution)
    {
        if (mFactory is null || mDocumentDock is null)
        {
            mPendingDocuments.Add(pContribution);
            return;
        }

        AddToDock(pContribution);
    }

    /// <inheritdoc/>
    public void CloseDocument(string pId)
    {
        if (mFactory is null || mDocumentDock is null)
        {
            return;
        }

        IDockable? lDockable = FindById(pId);
        if (lDockable is not null)
        {
            mFactory.CloseDockable(lDockable);
        }
    }

    /// <inheritdoc/>
    public void ActivateDocument(string pId)
    {
        if (mFactory is null || mDocumentDock is null)
        {
            return;
        }

        IDockable? lDockable = FindById(pId);
        if (lDockable is not null)
        {
            mFactory.SetActiveDockable(lDockable);
            mFactory.SetFocusedDockable(mDocumentDock, lDockable);
        }
    }

    /// <inheritdoc/>
    public bool IsDocumentOpen(string pId)
    {
        if (mDocumentDock is null)
        {
            return false;
        }

        return FindById(pId) is not null;
    }

    private void OnFactoryDockableClosed(object? pSender, DockableClosedEventArgs pArgs)
    {
        if (pArgs.Dockable?.Id is string lId)
        {
            DocumentClosed?.Invoke(this, lId);
        }

        // Return focus to the main window so the next menu-bar click executes
        // immediately rather than only re-focusing the window.
        Dispatcher.UIThread.Post(() =>
        {
            if (Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime lDesktop)
            {
                lDesktop.MainWindow?.Focus();
            }
        });
    }

    private void AddToDock(DocumentContribution pContribution)
    {
        IDockable lDockable = (IDockable)pContribution.ViewModel;
        mFactory!.AddDockable(mDocumentDock!, lDockable);
        mFactory.SetActiveDockable(lDockable);
        mFactory.SetFocusedDockable(mDocumentDock!, lDockable);
    }

    private IDockable? FindById(string pId)
    {
        return mDocumentDock!.VisibleDockables?.FirstOrDefault(pD => pD.Id == pId);
    }
}
