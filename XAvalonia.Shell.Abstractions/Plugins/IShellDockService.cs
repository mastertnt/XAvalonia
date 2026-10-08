namespace XAvalonia.Shell.Abstractions.Plugins;

/// <summary>
/// Allows a plugin to inject a dock layout into the shell's main window.
/// The layout is passed as <see cref="object"/> so that this abstraction has no
/// dependency on Dock.Avalonia.
/// </summary>
public interface IShellDockService
{
    /// <summary>
    /// Sets the root dock layout for the main window.
    /// The value must be an instance of <c>Dock.Model.Core.IRootDock</c>.
    /// </summary>
    /// <param name="pRootDock">The root dock layout produced by a <c>Dock.Model.ReactiveUI.Factory</c>.</param>
    void SetLayout(object pRootDock);
}
