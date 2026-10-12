namespace XAvalonia.Shell.Abstractions.Selection;

/// <summary>Describes a change of <see cref="ISelectionManager.ActiveContext"/>.</summary>
public sealed class ActiveSelectionContextChangedEventArgs : EventArgs
{
    /// <summary>Initializes the event arguments.</summary>
    /// <param name="pPreviousContext">Context that was active before the change.</param>
    /// <param name="pCurrentContext">Context that is active now.</param>
    public ActiveSelectionContextChangedEventArgs(ISelectionContext pPreviousContext, ISelectionContext pCurrentContext)
    {
        PreviousContext = pPreviousContext;
        CurrentContext  = pCurrentContext;
    }

    /// <summary>Context that was active before the change.</summary>
    public ISelectionContext PreviousContext { get; }

    /// <summary>Context that is active now.</summary>
    public ISelectionContext CurrentContext { get; }
}
