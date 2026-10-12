using XAvalonia.Shell.Abstractions.Selection;

namespace XAvalonia.Bootstrap.Services;

/// <summary>
/// <see cref="ISelectionManager"/> implementation.
/// Registered as singleton under both <c>SelectionManager</c> and <c>ISelectionManager</c>.
/// </summary>
public sealed class SelectionManager : ISelectionManager
{
    private readonly List<SelectionContext> mContexts = new();
    private SelectionContext mActiveContext;

    /// <summary>Initializes the manager with an empty, active global context.</summary>
    public SelectionManager()
    {
        SelectionContext lGlobal = new SelectionContext(ISelectionManager.GlobalContextId);
        lGlobal.SelectionChanged += OnContextSelectionChanged;
        mContexts.Add(lGlobal);
        mActiveContext = lGlobal;
        Contexts = mContexts.AsReadOnly();
    }

    /// <inheritdoc/>
    public ISelectionContext GlobalContext => mContexts[0];

    /// <inheritdoc/>
    public ISelectionContext ActiveContext => mActiveContext;

    /// <inheritdoc/>
    public IReadOnlyList<ISelectionContext> Contexts { get; }

    /// <inheritdoc/>
    public event EventHandler<ActiveSelectionContextChangedEventArgs>? ActiveContextChanged;

    /// <inheritdoc/>
    public event EventHandler<SelectionChangedEventArgs>? ActiveSelectionChanged;

    /// <inheritdoc/>
    public ISelectionContext GetOrCreateContext(string pId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pId);

        SelectionContext? lContext = Find(pId);
        if (lContext is null)
        {
            lContext = new SelectionContext(pId);
            lContext.SelectionChanged += OnContextSelectionChanged;
            mContexts.Add(lContext);
        }

        return lContext;
    }

    /// <inheritdoc/>
    public ISelectionContext? GetContext(string pId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pId);
        return Find(pId);
    }

    /// <inheritdoc/>
    public bool RemoveContext(string pId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pId);
        if (pId == ISelectionManager.GlobalContextId)
        {
            throw new InvalidOperationException("The global selection context cannot be removed.");
        }

        SelectionContext? lContext = Find(pId);
        if (lContext is null)
        {
            return false;
        }

        if (ReferenceEquals(lContext, mActiveContext))
        {
            ActivateGlobalContext();
        }

        lContext.SelectionChanged -= OnContextSelectionChanged;
        mContexts.Remove(lContext);
        return true;
    }

    /// <inheritdoc/>
    public void ActivateContext(ISelectionContext pContext)
    {
        ArgumentNullException.ThrowIfNull(pContext);
        if (pContext is not SelectionContext lContext || !mContexts.Contains(lContext))
        {
            throw new ArgumentException($"Selection context '{pContext.Id}' does not belong to this manager.", nameof(pContext));
        }

        if (ReferenceEquals(lContext, mActiveContext))
        {
            return;
        }

        SelectionContext lPrevious = mActiveContext;
        mActiveContext = lContext;

        ActiveContextChanged?.Invoke(this, new ActiveSelectionContextChangedEventArgs(lPrevious, lContext));

        HashSet<object> lPreviousSet = new(lPrevious.SelectedItems);
        HashSet<object> lCurrentSet = new(lContext.SelectedItems);
        List<object> lAdded = lContext.SelectedItems.Where(pItem => !lPreviousSet.Contains(pItem)).ToList();
        List<object> lRemoved = lPrevious.SelectedItems.Where(pItem => !lCurrentSet.Contains(pItem)).ToList();
        if (lAdded.Count > 0 || lRemoved.Count > 0)
        {
            ActiveSelectionChanged?.Invoke(this, new SelectionChangedEventArgs(lContext, lAdded, lRemoved));
        }
    }

    /// <inheritdoc/>
    public ISelectionContext ActivateContext(string pId)
    {
        ISelectionContext lContext = GetOrCreateContext(pId);
        ActivateContext(lContext);
        return lContext;
    }

    /// <inheritdoc/>
    public void ActivateGlobalContext() => ActivateContext(GlobalContext);

    private SelectionContext? Find(string pId) => mContexts.FirstOrDefault(pContext => pContext.Id == pId);

    // Relays changes of the active context only.
    private void OnContextSelectionChanged(object? pSender, SelectionChangedEventArgs pArgs)
    {
        if (ReferenceEquals(pSender, mActiveContext))
        {
            ActiveSelectionChanged?.Invoke(this, pArgs);
        }
    }
}
