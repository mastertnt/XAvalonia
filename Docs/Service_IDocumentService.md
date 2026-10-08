# IDocumentService

Manages document tabs in the central dock area. Documents are the primary editing surfaces of the shell — each one appears as a closeable tab.

---

## Interface

```csharp
public interface IDocumentService
{
    void RegisterDocument(DocumentContribution pContribution);
    void CloseDocument(string pId);
    void ActivateDocument(string pId);
    bool IsDocumentOpen(string pId);

    event EventHandler<string>? DocumentClosed;
}
```

---

## How to obtain

```csharp
IDocumentService lDocService = pServiceManager.RequestService<IDocumentService>();
```

---

## DocumentContribution

```csharp
public sealed class DocumentContribution
{
    public DocumentContribution(string pId, string pTitle, object pViewModel)
}

public string Id { get; }          // must match pViewModel.Id exactly
public string Title { get; }       // tab label
public object ViewModel { get; }   // must implement IDockable (extends Document)
```

> `pId` and `pViewModel.Id` **must be identical**. The dock framework uses the ViewModel's `Id` for all lookups — a mismatch causes `IsDocumentOpen` to always return `false`.

---

## ViewModel requirements

The ViewModel must extend `Dock.Model.ReactiveUI.Controls.Document`:

```csharp
using Dock.Model.ReactiveUI.Controls;

public sealed class MyDocumentViewModel : Document
{
    public MyDocumentViewModel()
    {
        Id    = "my-document";   // stable, unique
        Title = "My Document";
        CanClose = true;
        CanPin   = true;
        CanFloat = true;
    }
}
```

---

## View resolution

The shell's `ViewLocator` resolves the view by replacing `ViewModel` with `View` in the full type name:

```
MyPlugin.ViewModels.MyDocumentViewModel  →  MyPlugin.Views.MyDocumentView
```

The view must be a `UserControl` with a public parameterless constructor.

---

## Example — open once, re-activate if already open

The standard pattern for a plugin that opens a single document tab:

```csharp
private IDocumentService? mDocumentService;

public void Initialize(IPluginServiceManager pServiceManager)
{
    mDocumentService = pServiceManager.RequestService<IDocumentService>();

    IMenuService lMenu = pServiceManager.RequestService<IMenuService>();
    lMenu.RegisterMenu(new MenuContribution("view", "_View", 300));
    lMenu.RegisterMenuItem("view", new MenuItemContribution(
        pId: "view.dashboard",
        pHeader: "_Dashboard",
        pCommand: new RelayCommand(OpenDashboard)));

    mDocumentService.DocumentClosed += OnDocumentClosed;
}

private DashboardViewModel? mViewModel;

private void OpenDashboard()
{
    if (mDocumentService!.IsDocumentOpen("dashboard"))
    {
        mDocumentService.ActivateDocument("dashboard");
        return;
    }

    mViewModel = new DashboardViewModel();
    mDocumentService.RegisterDocument(new DocumentContribution(
        pId: "dashboard",
        pTitle: "Dashboard",
        pViewModel: mViewModel));
}

private void OnDocumentClosed(object? pSender, string pId)
{
    if (pId == "dashboard")
    {
        mViewModel = null;
    }
}
```

---

## Example — multiple independent documents

```csharp
private int mDocCounter = 0;

private void NewDocument()
{
    string lId    = $"doc-{++mDocCounter}";
    string lTitle = $"Document {mDocCounter}";

    mDocumentService!.RegisterDocument(new DocumentContribution(
        pId: lId,
        pTitle: lTitle,
        pViewModel: new EditorViewModel(lId, lTitle)));
}
```

---

## Example — close a document programmatically

```csharp
mDocumentService.CloseDocument("dashboard");
```

This triggers the `DocumentClosed` event with the document's Id.

---

## Example — react to document close

Use the `DocumentClosed` event to clean up state when the user closes a tab:

```csharp
mDocumentService.DocumentClosed += (pSender, pId) =>
{
    if (pId == "dashboard")
    {
        mViewModel = null;
        lStatusBar.UpdateItem("status", "Dashboard closed");
    }
};
```

---

## Queueing behaviour

Documents registered **before the dock is connected** (e.g., during early `Initialize`) are queued automatically and flushed once the layout is ready. No special handling is required — just call `RegisterDocument` normally.

---

## Document tab AXAML skeleton

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:MyPlugin.ViewModels"
             x:Class="MyPlugin.Views.MyDocumentView"
             x:DataType="vm:MyDocumentViewModel">

  <!-- Your document content here -->
  <Grid>
    <TextBlock Text="{Binding Title}"
               HorizontalAlignment="Center"
               VerticalAlignment="Center" />
  </Grid>

</UserControl>
```

---

## Summary of IDocumentService methods

| Method | Description |
|---|---|
| `RegisterDocument` | Opens a new tab; queued if dock not yet ready |
| `ActivateDocument` | Brings an existing tab to the front |
| `CloseDocument` | Programmatically closes a tab |
| `IsDocumentOpen` | Returns `true` if the tab is currently open |
| `DocumentClosed` | Fired when any tab is closed (user or programmatic) |
