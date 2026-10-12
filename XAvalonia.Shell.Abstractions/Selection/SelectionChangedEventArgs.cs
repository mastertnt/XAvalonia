namespace XAvalonia.Shell.Abstractions.Selection;

/// <summary>
/// Describes a selection change, either inside one <see cref="ISelectionContext"/>
/// or, for <see cref="ISelectionManager.ActiveSelectionChanged"/>, a switch of active context.
/// </summary>
public sealed class SelectionChangedEventArgs : EventArgs
{
    /// <summary>Initializes the event arguments.</summary>
    /// <param name="pContext">Context whose selection is now current.</param>
    /// <param name="pAddedItems">Items that became selected.</param>
    /// <param name="pRemovedItems">Items that are no longer selected.</param>
    public SelectionChangedEventArgs(ISelectionContext pContext, IReadOnlyList<object> pAddedItems, IReadOnlyList<object> pRemovedItems)
    {
        Context      = pContext;
        AddedItems   = pAddedItems;
        RemovedItems = pRemovedItems;
    }

    /// <summary>Context whose selection is now current.</summary>
    public ISelectionContext Context { get; }

    /// <summary>Items that became selected.</summary>
    public IReadOnlyList<object> AddedItems { get; }

    /// <summary>Items that are no longer selected.</summary>
    public IReadOnlyList<object> RemovedItems { get; }

    /// <summary>Selection after the change (shortcut for <c>Context.SelectedItems</c>).</summary>
    public IReadOnlyList<object> SelectedItems => Context.SelectedItems;
}
