using System.Collections.ObjectModel;
using XAvalonia.Bootstrap.ViewModels.StatusBar;
using XAvalonia.Shell.Abstractions.StatusBar;

namespace XAvalonia.Bootstrap.Services;

/// <summary>
/// <see cref="IStatusBarService"/> implementation for the main status bar.
/// Registered as singleton under both <c>StatusBarService</c> and <c>IStatusBarService</c>.
/// </summary>
public class StatusBarService : IStatusBarService
{
    private readonly ObservableCollection<StatusBarItemViewModel> mLeftItems = new();
    private readonly ObservableCollection<StatusBarItemViewModel> mRightItems = new();

    /// <summary>Initializes the service and wraps internal collections as read-only.</summary>
    public StatusBarService()
    {
        LeftItems  = new ReadOnlyObservableCollection<StatusBarItemViewModel>(mLeftItems);
        RightItems = new ReadOnlyObservableCollection<StatusBarItemViewModel>(mRightItems);
    }

    /// <summary>Read-only observable list of items on the left side of the status bar.</summary>
    public ReadOnlyObservableCollection<StatusBarItemViewModel> LeftItems { get; }

    /// <summary>Read-only observable list of items on the right side of the status bar.</summary>
    public ReadOnlyObservableCollection<StatusBarItemViewModel> RightItems { get; }

    /// <inheritdoc/>
    public void RegisterItem(StatusBarItemContribution pItem)
    {
        ObservableCollection<StatusBarItemViewModel> lTarget = SelectCollection(pItem.Alignment);

        if (lTarget.Any(pVm => pVm.Id == pItem.Id))
        {
            throw new InvalidOperationException($"Status bar item '{pItem.Id}' is already registered.");
        }

        StatusBarItemViewModel lVm = new StatusBarItemViewModel(pItem);
        int lInsertIndex = lTarget.Count(pVm => pVm.Order <= lVm.Order);
        lTarget.Insert(lInsertIndex, lVm);
    }

    /// <inheritdoc/>
    public void UnregisterItem(string pId)
    {
        StatusBarItemViewModel? lVm = mLeftItems.FirstOrDefault(pVm => pVm.Id == pId)
                                  ?? mRightItems.FirstOrDefault(pVm => pVm.Id == pId);
        if (lVm is null)
        {
            return;
        }

        if (!mLeftItems.Remove(lVm))
        {
            mRightItems.Remove(lVm);
        }
    }

    /// <inheritdoc/>
    public void UpdateItem(string pId, string pText)
    {
        StatusBarItemViewModel? lVm = mLeftItems.FirstOrDefault(pVm => pVm.Id == pId)
                                  ?? mRightItems.FirstOrDefault(pVm => pVm.Id == pId);
        if (lVm is not null)
        {
            lVm.Text = pText;
        }
    }

    private ObservableCollection<StatusBarItemViewModel> SelectCollection(StatusBarItemAlignment pAlignment)
    {
        return pAlignment == StatusBarItemAlignment.Right ? mRightItems : mLeftItems;
    }
}
