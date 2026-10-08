using XAvalonia.Shell.Abstractions.Logging;

namespace XAvalonia.LogViewer;

/// <summary>
/// Default implementation of <see cref="ILogService"/>.
/// Events are raised on the calling thread; consumers are responsible for marshalling to the UI thread.
/// </summary>
public sealed class LogService : ILogService
{
    /// <inheritdoc/>
    public event EventHandler<LogMessageEventArgs>? MessageLogged;

    /// <inheritdoc/>
    public event EventHandler? Cleared;

    /// <inheritdoc/>
    public void LogDebug(string pMessage)
    {
        Emit(LogLevel.Debug, pMessage);
    }

    /// <inheritdoc/>
    public void LogInfo(string pMessage)
    {
        Emit(LogLevel.Info, pMessage);
    }

    /// <inheritdoc/>
    public void LogWarning(string pMessage)
    {
        Emit(LogLevel.Warning, pMessage);
    }

    /// <inheritdoc/>
    public void LogError(string pMessage)
    {
        Emit(LogLevel.Error, pMessage);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        Cleared?.Invoke(this, EventArgs.Empty);
    }

    private void Emit(LogLevel pLevel, string pMessage)
    {
        MessageLogged?.Invoke(this, new LogMessageEventArgs(pLevel, pMessage));
    }
}
