using Dock.Model.Core;
using XAvalonia.Shell.Abstractions.ToolPanels;

namespace XAvalonia.Bootstrap.Services;

/// <summary>
/// <see cref="IToolPanelService"/> implementation that manages tool panels in the Dock layout.
/// Registered as singleton under <c>ToolPanelService</c>, <c>IToolPanelService</c>,
/// and <c>IToolPanelDockConnector</c>.
/// <para>
/// Panels registered before <see cref="ConnectDocks"/> is called are kept in memory
/// and flushed automatically when the dock context becomes available.
/// Calling <see cref="ConnectDocks"/> a second time (layout reset) re-adds all panels
/// to the freshly created docks.
/// </para>
/// </summary>
public sealed class ToolPanelService : IToolPanelService, IToolPanelDockConnector
{
    private readonly List<ToolPanelContribution> mAllPanels = new List<ToolPanelContribution>();
    private IFactory? mFactory;
    private IDock? mLeftDock;
    private IDock? mRightDock;
    private IDock? mBottomDock;

    /// <inheritdoc/>
    public void ConnectDocks(object pFactory, object? pLeftDock, object? pRightDock, object? pBottomDock)
    {
        mFactory    = (IFactory)pFactory;
        mLeftDock   = pLeftDock   is null ? null : (IDock)pLeftDock;
        mRightDock  = pRightDock  is null ? null : (IDock)pRightDock;
        mBottomDock = pBottomDock is null ? null : (IDock)pBottomDock;

        foreach (ToolPanelContribution lPanel in mAllPanels)
        {
            AddToDock(lPanel);
        }
    }

    /// <inheritdoc/>
    public void RegisterPanel(ToolPanelContribution pContribution)
    {
        mAllPanels.Add(pContribution);

        if (mFactory is not null)
        {
            AddToDock(pContribution);
        }
    }

    /// <inheritdoc/>
    public void UnregisterPanel(string pId)
    {
        ToolPanelContribution? lContribution = mAllPanels.FirstOrDefault(pP => pP.Id == pId);
        if (lContribution is null)
        {
            return;
        }

        mAllPanels.Remove(lContribution);

        if (mFactory is null)
        {
            return;
        }

        IDockable? lDockable = FindInDocks(pId);
        if (lDockable is not null)
        {
            mFactory.CloseDockable(lDockable);
        }
    }

    private void AddToDock(ToolPanelContribution pContribution)
    {
        IDock? lTargetDock = SelectDock(pContribution.Alignment);
        if (lTargetDock is null)
        {
            return;
        }

        IDockable lDockable = (IDockable)pContribution.ViewModel;
        mFactory!.AddDockable(lTargetDock, lDockable);
        mFactory.SetActiveDockable(lDockable);
        mFactory.SetFocusedDockable(lTargetDock, lDockable);
    }

    private IDock? SelectDock(ToolPanelAlignment pAlignment)
    {
        return pAlignment switch
        {
            ToolPanelAlignment.Left   => mLeftDock,
            ToolPanelAlignment.Right  => mRightDock,
            ToolPanelAlignment.Bottom => mBottomDock,
            _                         => null
        };
    }

    private IDockable? FindInDocks(string pId)
    {
        return FindInDock(mLeftDock, pId)
            ?? FindInDock(mRightDock, pId)
            ?? FindInDock(mBottomDock, pId);
    }

    private static IDockable? FindInDock(IDock? pDock, string pId)
    {
        return pDock?.VisibleDockables?.FirstOrDefault(pD => pD.Id == pId);
    }
}
