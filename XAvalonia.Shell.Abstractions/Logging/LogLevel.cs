namespace XAvalonia.Shell.Abstractions.Logging;

/// <summary>Severity level of a log message.</summary>
public enum LogLevel
{
    /// <summary>Detailed diagnostic information.</summary>
    Debug,

    /// <summary>General informational messages.</summary>
    Info,

    /// <summary>Non-critical anomalies that may require attention.</summary>
    Warning,

    /// <summary>Errors that indicate a failure in the current operation.</summary>
    Error,
}
