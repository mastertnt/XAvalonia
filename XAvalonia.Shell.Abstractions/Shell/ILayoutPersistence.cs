namespace XAvalonia.Shell.Abstractions.Shell;

/// <summary>
/// Provides layout persistence capabilities to the shell.
/// Implemented by the active layout plugin and consumed by <see cref="IMainWindowService"/>.
/// </summary>
public interface ILayoutPersistence
{
    /// <summary>
    /// Saves the current dock layout to the specified file.
    /// </summary>
    /// <param name="pFilePath">Full path of the file to write.</param>
    void SaveLayout(string pFilePath);

    /// <summary>
    /// Loads a previously saved dock layout from the specified file.
    /// No-op if the file does not exist.
    /// </summary>
    /// <param name="pFilePath">Full path of the file to read.</param>
    void LoadLayout(string pFilePath);
}
