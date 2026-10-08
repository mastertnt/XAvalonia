# IStatusBarService

Manages items in the shell's status bar — the thin strip at the bottom of the main window. Items can appear on the left or right side and can be updated at any time.

---

## Interface

```csharp
public interface IStatusBarService
{
    void RegisterItem(StatusBarItemContribution pItem);
    void UnregisterItem(string pId);
    void UpdateItem(string pId, string pText);
}
```

---

## How to obtain

```csharp
IStatusBarService lStatusBar = pServiceManager.RequestService<IStatusBarService>();
```

---

## StatusBarItemContribution

```csharp
public class StatusBarItemContribution
{
    public StatusBarItemContribution(
        string pId,
        string pText,
        StatusBarItemAlignment pAlignment = StatusBarItemAlignment.Left,
        int pOrder = 100,
        string? pIconUri = null)
}

public string Id { get; }
public string Text { get; }
public StatusBarItemAlignment Alignment { get; }
public int Order { get; }
public string? IconUri { get; }   // avares:// URI or absolute path
```

## StatusBarItemAlignment

```csharp
public enum StatusBarItemAlignment
{
    Left,
    Right
}
```

---

## Example — simple text item

```csharp
lStatusBar.RegisterItem(new StatusBarItemContribution(
    pId: "myplugin.ready",
    pText: "My Plugin ready",
    pAlignment: StatusBarItemAlignment.Left,
    pOrder: 200));
```

---

## Example — update text at runtime

`UpdateItem` can be called from any thread — it is thread-safe via the UI dispatcher:

```csharp
// On startup
lStatusBar.RegisterItem(new StatusBarItemContribution(
    pId: "myplugin.status",
    pText: "Idle",
    pAlignment: StatusBarItemAlignment.Left,
    pOrder: 300));

// During processing
lStatusBar.UpdateItem("myplugin.status", "Processing…");

// When done
lStatusBar.UpdateItem("myplugin.status", "Done");
```

---

## Example — right-aligned item (version, cursor position, etc.)

```csharp
lStatusBar.RegisterItem(new StatusBarItemContribution(
    pId: "myplugin.version",
    pText: "v1.0.0",
    pAlignment: StatusBarItemAlignment.Right,
    pOrder: 100));
```

---

## Example — item with icon

```csharp
lStatusBar.RegisterItem(new StatusBarItemContribution(
    pId: "myplugin.connection",
    pText: "Connected",
    pAlignment: StatusBarItemAlignment.Left,
    pOrder: 150,
    pIconUri: "avares://MyPlugin/Assets/connected.png"));
```

---

## Example — dynamic status tied to a background task

```csharp
public void Initialize(IPluginServiceManager pServiceManager)
{
    IStatusBarService lStatusBar = pServiceManager.RequestService<IStatusBarService>();

    lStatusBar.RegisterItem(new StatusBarItemContribution(
        pId: "myplugin.index",
        pText: "Index: idle",
        pAlignment: StatusBarItemAlignment.Left,
        pOrder: 400));

    // Start a background task that updates the status bar
    Task.Run(() => RebuildIndex(lStatusBar));
}

private async Task RebuildIndex(IStatusBarService pStatusBar)
{
    pStatusBar.UpdateItem("myplugin.index", "Index: rebuilding…");
    await Task.Delay(3000);   // simulate work
    pStatusBar.UpdateItem("myplugin.index", "Index: ready");
}
```

---

## Example — unregistering an item

```csharp
lStatusBar.UnregisterItem("myplugin.status");
```

---

## Ordering reference

Status bar items from all plugins are merged and sorted by `Order` within each alignment zone:

```
[Left zone]   order 100 → order 200 → order 300 …   [Right zone]  … order 300 → order 200 → order 100
```

Use low order values (100–200) for items that should appear close to the left or right edge.
