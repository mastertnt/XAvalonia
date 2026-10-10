using XAvalonia.Shell.Abstractions.Selection;

namespace XAvalonia.Bootstrap.Services;

/// <summary>
/// <see cref="ISelectionContext"/> implementation created by <see cref="SelectionManager"/>.
/// </summary>
public sealed class SelectionContext : ISelectionContext
{
    private static readonly IReadOnlyList<object> Empty = Array.Empty<object>();

    private readonly List<object> mItems = new();
    private readonly HashSet<object> mItemSet = new();

    /// <summary>Initializes an empty context.</summary>
    /// <param name="pId">Context identifier.</param>
    public SelectionContext(string pId)
    {
        Id = pId;
        SelectedItems = mItems.AsReadOnly();
    }

    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public IReadOnlyList<object> SelectedItems { get; }

    /// <inheritdoc/>
    public object? PrimaryItem => mItems.Count == 0 ? null : mItems[^1];

    /// <inheritdoc/>
    public bool IsEmpty => mItems.Count == 0;

    /// <inheritdoc/>
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    /// <inheritdoc/>
    public IEnumerable<T> GetSelected<T>() => mItems.OfType<T>();

    /// <inheritdoc/>
    public bool IsSelected(object pItem)
    {
        ArgumentNullException.ThrowIfNull(pItem);
        return mItemSet.Contains(pItem);
    }

    /// <inheritdoc/>
    public void Select(object pItem)
    {
        ArgumentNullException.ThrowIfNull(pItem);
        Select(new[] { pItem });
    }

    /// <inheritdoc/>
    public void Select(IEnumerable<object> pItems)
    {
        ArgumentNullException.ThrowIfNull(pItems);

        List<object> lNewItems = new();
        HashSet<object> lNewSet = new();
        foreach (object lItem in pItems)
        {
            ArgumentNullException.ThrowIfNull(lItem, nameof(pItems));
            if (lNewSet.Add(lItem))
            {
                lNewItems.Add(lItem);
            }
        }

        if (lNewItems.SequenceEqual(mItems))
        {
            return;
        }

        List<object> lRemoved = mItems.Where(pItem => !lNewSet.Contains(pItem)).ToList();
        List<object> lAdded = lNewItems.Where(pItem => !mItemSet.Contains(pItem)).ToList();

        mItems.Clear();
        mItems.AddRange(lNewItems);
        mItemSet.Clear();
        mItemSet.UnionWith(lNewSet);

        RaiseSelectionChanged(lAdded, lRemoved);
    }

    /// <inheritdoc/>
    public void Add(object pItem)
    {
        ArgumentNullException.ThrowIfNull(pItem);
        if (!mItemSet.Add(pItem))
        {
            return;
        }

        mItems.Add(pItem);
        RaiseSelectionChanged(new[] { pItem }, Empty);
    }

    /// <inheritdoc/>
    public void Remove(object pItem)
    {
        ArgumentNullException.ThrowIfNull(pItem);
        if (!mItemSet.Remove(pItem))
        {
            return;
        }

        mItems.Remove(pItem);
        RaiseSelectionChanged(Empty, new[] { pItem });
    }

    /// <inheritdoc/>
    public void Toggle(object pItem)
    {
        if (IsSelected(pItem))
        {
            Remove(pItem);
        }
        else
        {
            Add(pItem);
        }
    }

    /// <inheritdoc/>
    public void Clear()
    {
        if (mItems.Count == 0)
        {
            return;
        }

        object[] lRemoved = mItems.ToArray();
        mItems.Clear();
        mItemSet.Clear();
        RaiseSelectionChanged(Empty, lRemoved);
    }

    private void RaiseSelectionChanged(IReadOnlyList<object> pAdded, IReadOnlyList<object> pRemoved)
        => SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(this, pAdded, pRemoved));
}
