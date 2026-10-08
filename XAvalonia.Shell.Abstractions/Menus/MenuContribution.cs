namespace XAvalonia.Shell.Abstractions.Menus;

/// <summary>Describes a top-level menu to add to the menu bar.</summary>
public sealed class MenuContribution
{
    /// <summary>
    /// Initializes a new menu contribution.
    /// </summary>
    /// <param name="pId">Unique identifier (e.g. "file", "edit").</param>
    /// <param name="pHeader">Displayed text; '_' prefixes the keyboard shortcut (e.g. "_File").</param>
    /// <param name="pOrder">Relative position; menus are sorted in ascending order.</param>
    public MenuContribution(string pId, string pHeader, int pOrder = 100)
    {
        Id = pId;
        Header = pHeader;
        Order = pOrder;
    }

    /// <summary>Unique menu identifier (e.g. "file", "edit").</summary>
    public string Id { get; }

    /// <summary>Text displayed in the menu bar. '_' prefixes the keyboard shortcut.</summary>
    public string Header { get; }

    /// <summary>Relative position; menus are sorted in ascending order.</summary>
    public int Order { get; }
}
