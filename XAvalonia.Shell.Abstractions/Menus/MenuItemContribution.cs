using System.Windows.Input;

namespace XAvalonia.Shell.Abstractions.Menus;

/// <summary>Describes an item to add inside an existing menu.</summary>
public class MenuItemContribution
{
    /// <summary>
    /// Initializes a new menu item contribution.
    /// </summary>
    /// <param name="pId">Unique identifier within the parent menu.</param>
    /// <param name="pHeader">Displayed text; '_' prefixes the mnemonic shortcut.</param>
    /// <param name="pCommand">Command executed on click.</param>
    /// <param name="pOrder">Relative position; items are sorted in ascending order.</param>
    /// <param name="pInputGesture">
    /// Keyboard shortcut displayed and triggered (e.g. "Ctrl+N"). Must follow the Avalonia <c>KeyGesture</c> format.
    /// </param>
    /// <param name="pIconUri">
    /// Optional icon URI. Supports <c>avares://</c> asset URIs and absolute file paths.
    /// </param>
    public MenuItemContribution(
        string pId,
        string pHeader,
        ICommand? pCommand = null,
        int pOrder = 100,
        string? pInputGesture = null,
        string? pIconUri = null)
    {
        Id = pId;
        Header = pHeader;
        Command = pCommand;
        Order = pOrder;
        InputGesture = pInputGesture;
        IconUri = pIconUri;
    }

    /// <summary>Unique identifier within the parent menu.</summary>
    public string Id { get; }

    /// <summary>Text displayed in the menu. '_' prefixes the mnemonic shortcut.</summary>
    public string Header { get; }

    /// <summary>Command executed on click.</summary>
    public ICommand? Command { get; }

    /// <summary>Relative position; items are sorted in ascending order.</summary>
    public int Order { get; }

    /// <summary>
    /// Keyboard shortcut displayed and triggered (e.g. "Ctrl+N").
    /// Must follow the Avalonia <c>KeyGesture</c> format.
    /// </summary>
    public string? InputGesture { get; }

    /// <summary>
    /// Optional icon URI. Supports <c>avares://</c> asset URIs and absolute file paths.
    /// <c>null</c> means no icon.
    /// </summary>
    public string? IconUri { get; }
}

/// <summary>Horizontal separator inside a menu. The header is not displayed.</summary>
public sealed class MenuSeparatorContribution : MenuItemContribution
{
    /// <summary>
    /// Initializes a separator.
    /// </summary>
    /// <param name="pId">Unique identifier within the parent menu.</param>
    /// <param name="pOrder">Relative position within the menu.</param>
    public MenuSeparatorContribution(string pId, int pOrder = 100)
        : base(pId, string.Empty, null, pOrder)
    {
    }
}
