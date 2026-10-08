# ILogService

A publish/subscribe logging service shared by the shell and all plugins. The built-in Log Viewer plugin subscribes to it and displays entries in a filterable bottom panel.

---

## Interface

```csharp
public interface ILogService
{
    event EventHandler<LogMessageEventArgs> MessageLogged;
    event EventHandler Cleared;

    void LogDebug(string pMessage);
    void LogInfo(string pMessage);
    void LogWarning(string pMessage);
    void LogError(string pMessage);
    void Clear();
}
```

---

## How to obtain

`ILogService` is provided by the **Log Viewer plugin**, not by the shell core. It may not be registered if that plugin is absent. Always use `TryRequestService`:

```csharp
ILogService? lLog = pServiceManager.TryRequestService<ILogService>();
lLog?.LogInfo("Plugin initialized.");
```

---

## LogLevel

```csharp
public enum LogLevel { Debug, Info, Warning, Error }
```

## LogMessageEventArgs

```csharp
public sealed class LogMessageEventArgs : EventArgs
{
    public LogLevel Level { get; }
    public string Message { get; }
}
```

---

## Example — basic logging throughout a plugin

```csharp
private ILogService? mLog;

public void Initialize(IPluginServiceManager pServiceManager)
{
    mLog = pServiceManager.TryRequestService<ILogService>();
    mLog?.LogInfo($"[MyPlugin] Initializing…");

    try
    {
        DoSetup(pServiceManager);
        mLog?.LogInfo($"[MyPlugin] Ready.");
    }
    catch (Exception lEx)
    {
        mLog?.LogError($"[MyPlugin] Initialization failed: {lEx.Message}");
        throw;
    }
}
```

> Prefix messages with `[PluginName]` to make them easy to filter in the Log Viewer.

---

## Example — log levels and when to use them

```csharp
// DEBUG — internal state, only useful when investigating a specific issue
mLog?.LogDebug($"[MyPlugin] Cache hit for key '{lKey}'");

// INFO — significant events in normal operation
mLog?.LogInfo($"[MyPlugin] File '{lPath}' loaded successfully.");

// WARNING — something unexpected but recoverable
mLog?.LogWarning($"[MyPlugin] Config key 'timeout' not found, using default 30s.");

// ERROR — an operation failed; the plugin may still continue
mLog?.LogError($"[MyPlugin] Failed to connect to server: {lEx.Message}");
```

---

## Example — subscribing to log messages in your own plugin

If your plugin needs to react to log events (e.g., to forward them to a file or a remote sink):

```csharp
public void Initialize(IPluginServiceManager pServiceManager)
{
    ILogService? lLog = pServiceManager.TryRequestService<ILogService>();
    if (lLog is null) { return; }

    lLog.MessageLogged += OnMessageLogged;
    lLog.Cleared       += OnCleared;
}

private void OnMessageLogged(object? pSender, LogMessageEventArgs pArgs)
{
    // Forward to a file, remote service, etc.
    string lLine = $"[{DateTime.Now:HH:mm:ss}] [{pArgs.Level}] {pArgs.Message}";
    File.AppendAllText("app.log", lLine + Environment.NewLine);
}

private void OnCleared(object? pSender, EventArgs pArgs)
{
    // Optionally clear your own sink too
}
```

---

## Example — implementing your own ILogService

If you want a different backend (structured logging, file rotation, etc.), register your own implementation in Phase 1:

```csharp
// In RegisterServices:
public void RegisterServices(IServiceCollection pServices)
{
    pServices.AddSingleton<ILogService, FileLogService>();
}

public IReadOnlyList<Type> ProvidedServices => new[] { typeof(ILogService) };

// Implementation:
public sealed class FileLogService : ILogService
{
    private readonly string mFilePath;

    public FileLogService()
    {
        mFilePath = Path.Combine(AppContext.BaseDirectory, "app.log");
    }

    public event EventHandler<LogMessageEventArgs>? MessageLogged;
    public event EventHandler? Cleared;

    public void LogDebug(string pMessage)   => Emit(LogLevel.Debug,   pMessage);
    public void LogInfo(string pMessage)    => Emit(LogLevel.Info,    pMessage);
    public void LogWarning(string pMessage) => Emit(LogLevel.Warning, pMessage);
    public void LogError(string pMessage)   => Emit(LogLevel.Error,   pMessage);

    public void Clear()
    {
        File.WriteAllText(mFilePath, string.Empty);
        Cleared?.Invoke(this, EventArgs.Empty);
    }

    private void Emit(LogLevel pLevel, string pMessage)
    {
        string lLine = $"[{DateTime.Now:HH:mm:ss}] [{pLevel}] {pMessage}";
        File.AppendAllText(mFilePath, lLine + Environment.NewLine);
        MessageLogged?.Invoke(this, new LogMessageEventArgs(pLevel, pMessage));
    }
}
```

> Raise `MessageLogged` even in your custom implementation — the Log Viewer listens to that event to display entries.

---

## Conventions

| Convention | Description |
|---|---|
| Prefix `[PluginName]` | Makes log entries easy to filter in the Log Viewer |
| Use `TryRequestService` | `ILogService` may not be registered — never use `RequestService` for it |
| Always log before rethrowing | Log the exception before `throw;` so the error appears in the viewer |
| Don't log in `RegisterServices` | The log service may not be available at Phase 1 |
