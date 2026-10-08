using Avalonia.Media;
using XAvalonia.Shell.Abstractions.Logging;

namespace XAvalonia.LogViewer;

/// <summary>Represents a single immutable log entry displayed in the log viewer.</summary>
public sealed class LogEntryViewModel
{
    private static readonly IBrush DebugBrush   = new SolidColorBrush(Color.Parse("#888888"));
    private static readonly IBrush InfoBrush    = new SolidColorBrush(Color.Parse("#D4D4D4"));
    private static readonly IBrush WarningBrush = new SolidColorBrush(Color.Parse("#FFA500"));
    private static readonly IBrush ErrorBrush   = new SolidColorBrush(Color.Parse("#FF4444"));

    /// <summary>Initializes the entry with its severity level and message text.</summary>
    /// <param name="pLevel">Severity of the message.</param>
    /// <param name="pMessage">Text content of the message.</param>
    public LogEntryViewModel(LogLevel pLevel, string pMessage)
    {
        Level     = pLevel;
        Message   = pMessage;
        Timestamp = DateTime.Now;
    }

    /// <summary>Severity level of this entry.</summary>
    public LogLevel Level { get; }

    /// <summary>Text content of this entry.</summary>
    public string Message { get; }

    /// <summary>Time at which the entry was created.</summary>
    public DateTime Timestamp { get; }

    /// <summary>Pre-formatted display string including timestamp, level tag and message.</summary>
    public string FormattedMessage => $"[{Timestamp:HH:mm:ss}] [{LevelTag}] {Message}";

    /// <summary>Short three-letter tag for the severity level.</summary>
    public string LevelTag => Level switch
    {
        LogLevel.Debug   => "DBG",
        LogLevel.Info    => "INF",
        LogLevel.Warning => "WRN",
        LogLevel.Error   => "ERR",
        _                => "???",
    };

    /// <summary>Foreground brush to use when rendering this entry.</summary>
    public IBrush LevelBrush => Level switch
    {
        LogLevel.Debug   => DebugBrush,
        LogLevel.Info    => InfoBrush,
        LogLevel.Warning => WarningBrush,
        LogLevel.Error   => ErrorBrush,
        _                => InfoBrush,
    };
}
