namespace XAvalonia.Shell.Abstractions.Splashscreen;

/// <summary>
/// Startup splash screen shown while plugins are initialized.
/// Any plugin can push a status message to it during <c>Initialize</c>.
/// All members are thread-safe: calls from a background thread are marshalled to the UI thread.
/// </summary>
public interface ISplashscreen
{
    /// <summary>Whether the splash screen window is currently displayed.</summary>
    bool IsVisible { get; }

    /// <summary>Last message pushed with <see cref="ShowMessage"/>, or <c>null</c> if none.</summary>
    string? CurrentMessage { get; }

    /// <summary>
    /// Fired after <see cref="CurrentMessage"/> changes.
    /// The argument is the new message.
    /// </summary>
    event EventHandler<string>? MessageChanged;

    /// <summary>
    /// Displays the splash screen. Does nothing if it is already shown or has already been closed.
    /// </summary>
    void Show();

    /// <summary>
    /// Displays <paramref name="pMessage"/> on the splash screen (e.g. <c>"Loading documents…"</c>).
    /// A message pushed before <see cref="Show"/> is kept and displayed once the splash screen appears.
    /// </summary>
    /// <param name="pMessage">Text to display.</param>
    void ShowMessage(string pMessage);

    /// <summary>
    /// Closes the splash screen. It cannot be shown again afterwards.
    /// The implementation also closes it automatically once the main window is loaded.
    /// </summary>
    void Close();
}
