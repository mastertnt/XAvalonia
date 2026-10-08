namespace XAvalonia.Shell.Abstractions.Logging;

/// <summary>Provides data for the <see cref="ILogService.MessageLogged"/> event.</summary>
public sealed class LogMessageEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance with the given level and message.
    /// </summary>
    /// <param name="pLevel">Severity of the message.</param>
    /// <param name="pMessage">Text of the message.</param>
    public LogMessageEventArgs(LogLevel pLevel, string pMessage)
    {
        Level   = pLevel;
        Message = pMessage;
    }

    /// <summary>Severity of the log message.</summary>
    public LogLevel Level { get; }

    /// <summary>Text content of the log message.</summary>
    public string Message { get; }
}
