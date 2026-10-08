namespace XAvalonia.Shell.Abstractions.Shell;

/// <summary>
/// Allows plugins to control basic properties of the shell's main window.
/// </summary>
public interface IMainWindowService
{
    /// <summary>
    /// Represents the default file path for the layout configuration file, relative to the application's base
    /// directory.
    /// </summary>
    /// <remarks>This path is constructed by combining the application's base directory with the file name
    /// "defaultLayout.xml". Use this value when a custom layout path is not specified.</remarks>
    public static readonly string DefaultLayoutPath = Path.Combine(System.AppContext.BaseDirectory, "defaultLayout.xml");

    /// <summary>
    /// Fired when the main window has been shown and is ready for interaction.
    /// </summary>
    event EventHandler? Loaded;

    /// <summary>
    /// Fired when the main window is about to close (before any cleanup).
    /// </summary>
    event EventHandler? AboutToQuit;

    /// <summary>
    /// Fired after the main window has been fully closed.
    /// </summary>
    event EventHandler? Closed;

    /// <summary>
    /// Sets the main window title shown in the title bar.
    /// </summary>
    /// <param name="pTitle">New title text.</param>
    void SetTitle(string pTitle);

    /// <summary>
    /// Sets the application icon shown in the title bar and task bar.
    /// Supports <c>avares://</c> asset URIs and absolute file paths.
    /// Pass <c>null</c> to clear the icon.
    /// </summary>
    /// <param name="pIconUri">URI of the icon image, or <c>null</c> to clear.</param>
    void SetIcon(string? pIconUri);

    /// <summary>
    /// Shuts down the application by closing the main window.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Saves the current dock layout to the specified file.
    /// No-op if no layout persistence provider is registered.
    /// </summary>
    /// <param name="pFilePath">Full path of the file to write.</param>
    void SaveLayout(string pFilePath);

    /// <summary>
    /// Loads a previously saved dock layout from the specified file.
    /// No-op if the file does not exist or no layout persistence provider is registered.
    /// </summary>
    /// <param name="pFilePath">Full path of the file to read.</param>
    void LoadLayout(string pFilePath);
}
