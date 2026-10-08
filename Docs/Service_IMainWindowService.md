# IMainWindowService

Provides access to the shell's main window — its title, icon, lifecycle events, and layout persistence. Use this service to react to application lifecycle events or to drive the window from a plugin.

---

## Interface

```csharp
public interface IMainWindowService
{
    event EventHandler? Loaded;
    event EventHandler? AboutToQuit;
    event EventHandler? Closed;

    void SetTitle(string pTitle);
    void SetIcon(string? pIconUri);
    void Shutdown();
    void SaveLayout(string pFilePath);
    void LoadLayout(string pFilePath);
}
```

---

## How to obtain

```csharp
IMainWindowService lWindowService = pServiceManager.RequestService<IMainWindowService>();
```

---

## Lifecycle events

| Event | When it fires |
|---|---|
| `Loaded` | Main window is shown and fully ready |
| `AboutToQuit` | User has requested close; before cleanup |
| `Closed` | Window has fully closed |

Subscribe in `Initialize` — never in `RegisterServices`:

```csharp
lWindowService.Loaded      += OnLoaded;
lWindowService.AboutToQuit += OnAboutToQuit;
lWindowService.Closed      += OnClosed;
```

---

## Example — set window title dynamically

```csharp
public void Initialize(IPluginServiceManager pServiceManager)
{
    IMainWindowService lWindow = pServiceManager.RequestService<IMainWindowService>();

    lWindow.SetTitle("My Application — Untitled");

    // Update when the active document changes
    mDocumentService!.DocumentClosed += (_, _) =>
        lWindow.SetTitle("My Application");
}
```

---

## Example — set window icon

```csharp
// From an embedded resource (avares:// URI)
lWindow.SetIcon("avares://MyPlugin/Assets/app-icon.ico");

// From an absolute path
lWindow.SetIcon(@"C:\MyApp\icon.png");

// Clear the icon
lWindow.SetIcon(null);
```

Supported formats: `.ico`, `.png`, `.bmp`.

---

## Example — save and restore layout

The shell can serialize the entire dock layout (panel positions, tab order, sizes) to XML:

```csharp
private const string LayoutPath = @"C:\MyApp\layout.xml";

public void Initialize(IPluginServiceManager pServiceManager)
{
    IMainWindowService lWindow = pServiceManager.RequestService<IMainWindowService>();

    // Restore layout when the window is ready
    lWindow.Loaded += (_, _) =>
    {
        if (File.Exists(LayoutPath))
        {
            lWindow.LoadLayout(LayoutPath);
        }
    };

    // Save layout before the app exits
    lWindow.AboutToQuit += (_, _) =>
    {
        lWindow.SaveLayout(LayoutPath);
    };
}
```

---

## Example — shutdown the application

```csharp
lWindow.Shutdown();
```

Equivalent to the user clicking the window's close button. The `AboutToQuit` and `Closed` events fire normally.

---

## Example — title reflects active document

```csharp
private readonly IMainWindowService mWindow;
private readonly IDocumentService mDocuments;
private string mAppName = "My Application";

public void Initialize(IPluginServiceManager pServiceManager)
{
    mWindow    = pServiceManager.RequestService<IMainWindowService>();
    mDocuments = pServiceManager.RequestService<IDocumentService>();

    mWindow.SetTitle(mAppName);

    mDocuments.DocumentClosed += OnDocumentClosed;
}

private void OnDocumentClosed(object? pSender, string pId)
{
    mWindow.SetTitle(mAppName);
}
```

---

## Default layout path

```csharp
public static readonly string DefaultLayoutPath =
    Path.Combine(AppContext.BaseDirectory, "defaultLayout.xml");
```

If you don't specify a path, use `IMainWindowService.DefaultLayoutPath` as a convention to ensure all plugins agree on the same file.
