namespace XAvalonia.Shell.Abstractions.Logging;

/// <summary>
/// Service that allows the shell and plugins to emit log messages
/// and be notified when new messages are available.
/// </summary>
public interface ILogService
{
    /// <summary>
    /// Raised whenever a new message is appended to the log.
    /// </summary>
    /// <remarks>
    /// The <see cref="LogMessageEventArgs"/> contains the message text and its severity level.
    /// </remarks>
    event EventHandler<LogMessageEventArgs> MessageLogged;

    /// <summary>Raised when the log is cleared via <see cref="Clear"/>.</summary>
    event EventHandler Cleared;

    /// <summary>Logs a debug-level message.</summary>
    /// <param name="pMessage">Message to log.</param>
    void LogDebug(string pMessage);

    /// <summary>Logs an informational message.</summary>
    /// <param name="pMessage">Message to log.</param>
    void LogInfo(string pMessage);

    /// <summary>Logs a warning-level message.</summary>
    /// <param name="pMessage">Message to log.</param>
    void LogWarning(string pMessage);

    /// <summary>Logs an error-level message.</summary>
    /// <param name="pMessage">Message to log.</param>
    void LogError(string pMessage);

    /// <summary>Clears all messages from the log.</summary>
    void Clear();
}
