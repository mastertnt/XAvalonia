namespace XAvalonia.Shell.Abstractions.StatusBar;

/// <summary>Describes a status bar item contributed by the shell or a plugin.</summary>
public class StatusBarItemContribution
{
    /// <summary>
    /// Initializes a new status bar item contribution.
    /// </summary>
    /// <param name="pId">Unique identifier used to update or remove this item.</param>
    /// <param name="pText">Initial display text.</param>
    /// <param name="pAlignment">Which side of the status bar the item appears on.</param>
    /// <param name="pOrder">Sort order within its side (ascending).</param>
    /// <param name="pIconUri">
    /// Optional icon URI. Supports <c>avares://</c> asset URIs and absolute file paths.
    /// </param>
    public StatusBarItemContribution(
        string pId,
        string pText,
        StatusBarItemAlignment pAlignment = StatusBarItemAlignment.Left,
        int pOrder = 100,
        string? pIconUri = null)
    {
        Id = pId;
        Text = pText;
        Alignment = pAlignment;
        Order = pOrder;
        IconUri = pIconUri;
    }

    /// <summary>Unique identifier for this item.</summary>
    public string Id { get; }

    /// <summary>Initial display text shown in the status bar.</summary>
    public string Text { get; }

    /// <summary>Side of the status bar where this item appears.</summary>
    public StatusBarItemAlignment Alignment { get; }

    /// <summary>Sort order within the item's side (ascending).</summary>
    public int Order { get; }

    /// <summary>
    /// Optional icon URI. Supports <c>avares://</c> asset URIs and absolute file paths.
    /// <c>null</c> means no icon.
    /// </summary>
    public string? IconUri { get; }
}
