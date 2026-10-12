# ISelectionManager

Shares "what is selected" between plugins. The shell owns one **global** selection context, always present, and plugins create **local** contexts when a part of the UI needs its own selection (one per document, one per tool panel…). Exactly one context is **active** at a time: consumers such as a properties panel follow the active context and don't need to know who produced the selection.

The implementation is a core service of `XAvalonia.Bootstrap`: it is always registered.

---

## Interfaces

```csharp
public interface ISelectionManager
{
    const string GlobalContextId = "global";

    ISelectionContext GlobalContext { get; }
    ISelectionContext ActiveContext { get; }
    IReadOnlyList<ISelectionContext> Contexts { get; }

    event EventHandler<ActiveSelectionContextChangedEventArgs>? ActiveContextChanged;
    event EventHandler<SelectionChangedEventArgs>? ActiveSelectionChanged;

    ISelectionContext GetOrCreateContext(string pId);
    ISelectionContext? GetContext(string pId);
    bool RemoveContext(string pId);

    void ActivateContext(ISelectionContext pContext);
    ISelectionContext ActivateContext(string pId);
    void ActivateGlobalContext();
}

public interface ISelectionContext
{
    string Id { get; }
    IReadOnlyList<object> SelectedItems { get; }
    object? PrimaryItem { get; }
    bool IsEmpty { get; }

    event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    IEnumerable<T> GetSelected<T>();
    bool IsSelected(object pItem);
    void Select(object pItem);
    void Select(IEnumerable<object> pItems);
    void Add(object pItem);
    void Remove(object pItem);
    void Toggle(object pItem);
    void Clear();
}
```

### ISelectionManager

| Member | Description |
|---|---|
| `GlobalContext` | Application-wide context (id `"global"`); cannot be removed |
| `ActiveContext` | Context currently in charge; `GlobalContext` by default |
| `Contexts` | All contexts, the global one first |
| `ActiveContextChanged` | Raised after another context becomes active |
| `ActiveSelectionChanged` | Raised when the active selection changes: the active context is modified **or** another context becomes active |
| `GetOrCreateContext(id)` | Returns the context with this id, creating it if needed |
| `GetContext(id)` | Returns the context, or `null` if it does not exist |
| `RemoveContext(id)` | Removes a local context; if it was active, the global context becomes active again |
| `ActivateContext(...)` | Makes a context active (by instance, or by id — created if needed) |
| `ActivateGlobalContext()` | Makes the global context active again |

### ISelectionContext

| Member | Description |
|---|---|
| `SelectedItems` | Selected items, in selection order |
| `PrimaryItem` | Last selected item, or `null` |
| `SelectionChanged` | Raised after this context's selection changes, with `AddedItems` / `RemovedItems` |
| `GetSelected<T>()` | Selected items of a given type |
| `Select(...)` | Replaces the selection (one item or several) |
| `Add` / `Remove` / `Toggle` / `Clear` | Edits the selection |

Items are any non-null objects, compared with their default equality; an item is selected at most once. Events are not raised when an operation leaves the selection unchanged.

---

## How to obtain

```csharp
ISelectionManager lSelection = pServiceManager.RequestService<ISelectionManager>();
```

---

## Example — producer writing to the global context

```csharp
// Explorer tree: the selected node becomes the application-wide selection.
public FileNode? SelectedNode
{
    get => mSelectedNode;
    set
    {
        this.RaiseAndSetIfChanged(ref mSelectedNode, value);
        if (value is null) mSelection.GlobalContext.Clear();
        else mSelection.GlobalContext.Select(value);
    }
}
```

## Example — consumer following the active selection

```csharp
// Properties panel: shows whatever is selected in the active context.
mSelection.ActiveSelectionChanged += (_, pArgs) =>
    ShowProperties(pArgs.Context.PrimaryItem);
```

`pArgs.Context` is the active context. When the active context switches, `AddedItems` / `RemovedItems` hold the difference between the previous and the new selection.

## Example — local context per document

```csharp
// When a document is created: give it its own selection.
ISelectionContext lDocSelection = mSelection.GetOrCreateContext(pDocumentId);
lDocSelection.Select(lShape);

// When the document gets the focus: its selection becomes the active one.
mSelection.ActivateContext(lDocSelection);

// When the focus goes back to the rest of the application.
mSelection.ActivateGlobalContext();

// When the document is closed.
mSelection.RemoveContext(pDocumentId);
```

A local context keeps its selection while it is inactive: re-activating it restores what was selected.

---

## Conventions

| Convention | Description |
|---|---|
| Write to your own context | A document or panel writes to its local context (or the global one), never to another plugin's context |
| Read the active context | Consumers subscribe to `ActiveSelectionChanged` instead of a specific context |
| Use stable ids | Use the document or panel id as context id (e.g. `"doc1"`, `"explorer"`) |
| Remove what you create | Call `RemoveContext` when the document or panel goes away |
| UI thread only | The service is not thread-safe; dispatch to the UI thread before changing a selection |
| Don't call it in `RegisterServices` | The service is only usable from Phase 2 |
