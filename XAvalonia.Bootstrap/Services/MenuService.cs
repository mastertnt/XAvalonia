using System.Collections.ObjectModel;
using XAvalonia.Bootstrap.ViewModels.Menu;
using XAvalonia.Shell.Abstractions.Menus;

namespace XAvalonia.Bootstrap.Services;

/// <summary>
/// <see cref="IMenuService"/> implementation for the main menu bar.
/// Registered as singleton under both <c>MenuService</c> and <c>IMenuService</c>.
/// </summary>
public class MenuService : IMenuService
{
    private readonly ObservableCollection<MenuViewModel> mMenus = new();

    /// <summary>Initializes the service and wraps the internal collection as read-only.</summary>
    public MenuService()
    {
        Menus = new ReadOnlyObservableCollection<MenuViewModel>(mMenus);
    }

    /// <summary>Read-only observable view of the top-level menu list; bound to the menu bar.</summary>
    public ReadOnlyObservableCollection<MenuViewModel> Menus { get; }

    /// <inheritdoc/>
    public void RegisterMenu(MenuContribution pContribution)
    {
        if (mMenus.Any(pM => pM.Id == pContribution.Id))
        {
            throw new InvalidOperationException($"Le menu '{pContribution.Id}' est déjà enregistré.");
        }

        MenuViewModel lVm = new MenuViewModel(pContribution.Id, pContribution.Header, pContribution.Order);
        int lInsertIndex = mMenus.Count(pM => pM.Order <= pContribution.Order);
        mMenus.Insert(lInsertIndex, lVm);
    }

    /// <inheritdoc/>
    public void RegisterMenuItem(string pMenuId, MenuItemContribution pItem)
    {
        MenuViewModel? lMenu = mMenus.FirstOrDefault(pM => pM.Id == pMenuId)
            ?? throw new InvalidOperationException($"Le menu '{pMenuId}' n'est pas enregistré.");

        lMenu.AddItem(pItem);
    }

    /// <inheritdoc/>
    public void UnregisterMenu(string pMenuId)
    {
        MenuViewModel? lVm = mMenus.FirstOrDefault(pM => pM.Id == pMenuId);
        if (lVm is not null)
        {
            mMenus.Remove(lVm);
        }
    }

    /// <inheritdoc/>
    public void UnregisterMenuItem(string pMenuId, string pItemId)
    {
        MenuViewModel? lMenu = mMenus.FirstOrDefault(pM => pM.Id == pMenuId);
        lMenu?.RemoveItem(pItemId);
    }
}
