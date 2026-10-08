namespace XAvalonia.Shell.Abstractions.Menus;

/// <summary>
/// Eclipse-style service for dynamic registration of menus in the main menu bar.
/// Contributions are sorted by their <see cref="MenuContribution.Order"/> property.
/// </summary>
public interface IMenuService
{
    /// <summary>Registers a top-level menu (e.g. "File", "Edit").</summary>
    void RegisterMenu(MenuContribution pMenu);

    /// <summary>Registers an item inside an existing menu.</summary>
    void RegisterMenuItem(string pMenuId, MenuItemContribution pItem);

    /// <summary>Removes a top-level menu and all its items.</summary>
    void UnregisterMenu(string pMenuId);

    /// <summary>Removes a specific item from a menu.</summary>
    void UnregisterMenuItem(string pMenuId, string pItemId);
}
