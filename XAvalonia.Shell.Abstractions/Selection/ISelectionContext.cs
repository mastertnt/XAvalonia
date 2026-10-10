namespace XAvalonia.Shell.Abstractions.Selection;

/// <summary>
/// An independent set of selected items, identified by <see cref="Id"/>.
/// The shell owns one global context; plugins create local contexts (e.g. one per document
/// or tool panel) through <see cref="ISelectionManager.GetOrCreateContext"/>.
/// Items are compared with <see cref="EqualityComparer{T}.Default"/>; an item is selected at most once.
/// Members are not thread-safe and are meant to be called from the UI thread.
/// </summary>
public interface ISelectionContext
{
    /// <summary>Unique identifier of the context (e.g. <c>"global"</c>, <c>"doc1"</c>).</summary>
    string Id { get; }

    /// <summary>Selected items, in selection order.</summary>
    IReadOnlyList<object> SelectedItems { get; }

    /// <summary>Last selected item, or <c>null</c> if the selection is empty.</summary>
    object? PrimaryItem { get; }

    /// <summary><c>true</c> when nothing is selected.</summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Fired after the selection of this context changes.
    /// Not fired when an operation leaves the selection unchanged.
    /// </summary>
    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    /// <summary>Returns the selected items assignable to <typeparamref name="T"/>, in selection order.</summary>
    /// <typeparam name="T">Type of the items to return.</typeparam>
    IEnumerable<T> GetSelected<T>();

    /// <summary>Whether <paramref name="pItem"/> is currently selected.</summary>
    /// <param name="pItem">Item to test.</param>
    bool IsSelected(object pItem);

    /// <summary>Replaces the selection with <paramref name="pItem"/> only.</summary>
    /// <param name="pItem">Item to select.</param>
    void Select(object pItem);

    /// <summary>Replaces the selection with <paramref name="pItems"/> (duplicates are ignored).</summary>
    /// <param name="pItems">Items to select; an empty sequence clears the selection.</param>
    void Select(IEnumerable<object> pItems);

    /// <summary>Adds <paramref name="pItem"/> to the selection. Does nothing if it is already selected.</summary>
    /// <param name="pItem">Item to add.</param>
    void Add(object pItem);

    /// <summary>Removes <paramref name="pItem"/> from the selection. Does nothing if it is not selected.</summary>
    /// <param name="pItem">Item to remove.</param>
    void Remove(object pItem);

    /// <summary>Adds <paramref name="pItem"/> if it is not selected, removes it otherwise.</summary>
    /// <param name="pItem">Item to toggle.</param>
    void Toggle(object pItem);

    /// <summary>Empties the selection.</summary>
    void Clear();
}
