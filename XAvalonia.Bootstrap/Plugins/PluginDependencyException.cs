namespace XAvalonia.Bootstrap.Plugins;

/// <summary>
/// Thrown when plugins cannot be ordered: duplicate ids, a missing required dependency, or a dependency cycle.
/// </summary>
public sealed class PluginDependencyException : Exception
{
    /// <param name="pMessage">Description of the dependency problem.</param>
    public PluginDependencyException(string pMessage)
        : base(pMessage)
    {
    }
}
