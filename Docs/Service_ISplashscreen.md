# ISplashscreen

Shows a splash screen while the shell starts: an image defined in the technical configuration, with a message line that any plugin can update during its initialization. The default implementation is provided by the `XAvalonia.Splashscreen` plugin.

---

## Interface

```csharp
public interface ISplashscreen
{
    bool IsVisible { get; }
    string? CurrentMessage { get; }

    event EventHandler<string>? MessageChanged;

    void Show();
    void ShowMessage(string pMessage);
    void Close();
}
```

| Member | Description |
|---|---|
| `IsVisible` | `true` while the splash screen window is displayed |
| `CurrentMessage` | Last message pushed with `ShowMessage` |
| `MessageChanged` | Raised (on the calling thread) each time a message is pushed |
| `Show()` | Displays the splash screen; no-op if already shown or already closed |
| `ShowMessage()` | Displays a status message at the bottom of the splash screen |
| `Close()` | Closes the splash screen for good |

All members can be called from any thread; UI work is marshalled to the UI thread.

---

## How to obtain

The splash screen is optional — an application may not ship the plugin — so always use `TryRequestService`:

```csharp
ISplashscreen? lSplash = pServiceManager.TryRequestService<ISplashscreen>();
```

---

## Example — pushing messages from a plugin

```csharp
public void Initialize(IPluginServiceManager pServiceManager)
{
    ISplashscreen? lSplash = pServiceManager.TryRequestService<ISplashscreen>();

    lSplash?.ShowMessage("Connecting to the database…");
    ConnectToDatabase();

    lSplash?.ShowMessage("Loading projects…");
    LoadProjects();
}
```

Plugins are initialized on the UI thread; the splash screen repaints each time a message is pushed, so the user sees progress even though startup is synchronous.

---

## Technical configuration

The plugin reads the `"Splashscreen"` section of `technical_configuration.json`:

```json
{
  "Splashscreen": {
    "enabled": true,
    "imagePath": "Assets/splash.png",
    "width": 600,
    "height": 340,
    "background": "#1E1E1E",
    "foreground": "#FFFFFF",
    "minimumDisplayTime": 1500
  }
}
```

| Key | Type | Default | Description |
|---|---|---|---|
| `enabled` | bool | `true` | `false` keeps the service registered but never shows the window (messages are ignored) |
| `imagePath` | string | `null` | `avares://` URI, absolute path, or path relative to the application directory |
| `width` | int | image width | Window width in pixels (480 when there is no image) |
| `height` | int | image height | Window height in pixels (270 when there is no image) |
| `background` | color | `#1E1E1E` | Window background, visible around or without the image |
| `foreground` | color | `#FFFFFF` | Message text color |
| `minimumDisplayTime` | int (ms) | `0` | Minimum time the splash screen stays visible, even if startup is faster |

The image is scaled uniformly to fit the window. If it cannot be loaded, a warning is written to the trace output and the splash screen shows the background color only.

---

## Lifecycle

1. `RegisterServices` registers the `ISplashscreen` singleton.
2. `Initialize` applies the configuration and shows the window.
3. When `IMainWindowService.Loaded` fires, the window closes (after `minimumDisplayTime` if it has not elapsed yet).

Plugins initialized **before** the splash screen plugin can already call `ShowMessage`: the last message is kept and displayed as soon as the window appears. To see the splash screen from the very first plugin, load the splash screen plugin first.

---

## Conventions

| Convention | Description |
|---|---|
| Use `TryRequestService` | `ISplashscreen` may not be registered |
| Short, user-facing messages | The message line is a single trimmed line: `"Loading projects…"`, not a log entry |
| Don't call it in `RegisterServices` | The service is only usable from Phase 2 |
| Don't close it yourself | The plugin closes it when the main window is loaded; call `Close()` only for a custom startup flow |
