namespace XAvalonia.Shell.Abstractions.Selection;

/// <summary>
/// Manages selection contexts: one global context, always present, and any number of local
/// contexts identified by id (e.g. one per document or tool panel).
/// Exactly one context is active at a time; it defaults to <see cref="GlobalContext"/>.
/// Consumers such as a properties panel follow <see cref="ActiveContext"/> through
/// <see cref="ActiveSelectionChanged"/>, producers write to their own context.
/// Members are not thread-safe and are meant to be called from the UI thread.
/// </summary>
public interface ISelectionManager
{
    /// <summary>Identifier of the global context.</summary>
    const string GlobalContextId = "global";

    /// <summary>The application-wide context. It cannot be removed.</summary>
    ISelectionContext GlobalContext { get; }

    /// <summary>The context currently in charge of the selection; <see cref="GlobalContext"/> by default.</summary>
    ISelectionContext ActiveContext { get; }

    /// <summary>All contexts, the global one first.</summary>
    IReadOnlyList<ISelectionContext> Contexts { get; }

    /// <summary>Fired after <see cref="ActiveContext"/> changes.</summary>
    event EventHandler<ActiveSelectionContextChangedEventArgs>? ActiveContextChanged;

    /// <summary>
    /// Fired whenever the active selection changes: either the selection of
    /// <see cref="ActiveContext"/> is modified, or another context becomes active.
    /// When the active context switches, <see cref="SelectionChangedEventArgs.AddedItems"/> and
    /// <see cref="SelectionChangedEventArgs.RemovedItems"/> are the difference between the two selections.
    /// </summary>
    event EventHandler<SelectionChangedEventArgs>? ActiveSelectionChanged;

    /// <summary>Returns the context named <paramref name="pId"/>, creating it if needed.</summary>
    /// <param name="pId">Context identifier; <see cref="GlobalContextId"/> returns <see cref="GlobalContext"/>.</param>
    ISelectionContext GetOrCreateContext(string pId);

    /// <summary>Returns the context named <paramref name="pId"/>, or <c>null</c> if it does not exist.</summary>
    /// <param name="pId">Context identifier.</param>
    ISelectionContext? GetContext(string pId);

    /// <summary>
    /// Removes the local context named <paramref name="pId"/> (e.g. when its document is closed).
    /// If it was active, <see cref="GlobalContext"/> becomes active again.
    /// </summary>
    /// <param name="pId">Context identifier.</param>
    /// <returns><c>false</c> if the context does not exist.</returns>
    /// <exception cref="InvalidOperationException">Thrown when trying to remove the global context.</exception>
    bool RemoveContext(string pId);

    /// <summary>Makes <paramref name="pContext"/> the active context. Does nothing if it already is.</summary>
    /// <param name="pContext">A context obtained from this manager.</param>
    /// <exception cref="ArgumentException">Thrown when the context does not belong to this manager.</exception>
    void ActivateContext(ISelectionContext pContext);

    /// <summary>Makes the context named <paramref name="pId"/> active, creating it if needed.</summary>
    /// <param name="pId">Context identifier.</param>
    /// <returns>The activated context.</returns>
    ISelectionContext ActivateContext(string pId);

    /// <summary>Makes <see cref="GlobalContext"/> active again.</summary>
    void ActivateGlobalContext();
}
