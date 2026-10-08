namespace XAvalonia.Shell.Abstractions.StatusBar;

/// <summary>
/// Service that allows the shell and plugins to manage status bar items dynamically.
/// </summary>
public interface IStatusBarService
{
    /// <summary>
    /// Registers a new item in the status bar.
    /// </summary>
    /// <param name="pItem">Contribution describing the item to add.</param>
    /// <exception cref="System.InvalidOperationException">
    /// An item with the same <see cref="StatusBarItemContribution.Id"/> is already registered.
    /// </exception>
    void RegisterItem(StatusBarItemContribution pItem);

    /// <summary>
    /// Removes a previously registered item from the status bar.
    /// No-op if the item does not exist.
    /// </summary>
    /// <param name="pId">Identifier of the item to remove.</param>
    void UnregisterItem(string pId);

    /// <summary>
    /// Updates the display text of a registered item.
    /// No-op if the item does not exist.
    /// </summary>
    /// <param name="pId">Identifier of the item to update.</param>
    /// <param name="pText">New text to display.</param>
    void UpdateItem(string pId, string pText);
}
