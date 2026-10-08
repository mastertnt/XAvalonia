using System.Collections.ObjectModel;
using XAvalonia.Shell.Abstractions.Menus;

namespace XAvalonia.Bootstrap.ViewModels.Menu;

/// <summary>View model for a top-level menu entry in the menu bar.</summary>
public class MenuViewModel
{
    private readonly ObservableCollection<MenuItemViewModel> mItems = new();

    internal MenuViewModel(string pId, string pHeader, int pOrder)
    {
        Id     = pId;
        Header = pHeader;
        Order  = pOrder;
        Items  = new ReadOnlyObservableCollection<MenuItemViewModel>(mItems);
    }

    /// <summary>Unique identifier used by <see cref="IMenuService"/>.</summary>
    public string Id { get; }

    /// <summary>Display text shown in the menu bar (supports access-key underscore, e.g. <c>_File</c>).</summary>
    public string Header { get; }

    /// <summary>Sort order among top-level menus.</summary>
    public int Order { get; }

    /// <summary>Observable list of items within this menu.</summary>
    public ReadOnlyObservableCollection<MenuItemViewModel> Items { get; }

    internal void AddItem(MenuItemContribution pContribution)
    {
        MenuItemViewModel lVm = new MenuItemViewModel(pContribution);
        int lInsertIndex = mItems.Count(pExisting => pExisting.Order <= lVm.Order);
        mItems.Insert(lInsertIndex, lVm);
    }

    internal void RemoveItem(string pItemId)
    {
        MenuItemViewModel? lItem = mItems.FirstOrDefault(pI => pI.Id == pItemId);
        if (lItem is not null)
        {
            mItems.Remove(lItem);
        }
    }
}
