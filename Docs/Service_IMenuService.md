# IMenuService

Allows plugins to contribute top-level menus and menu items to the shell's menu bar at runtime. Menus are sorted by `Order` and rendered in the main window's menu bar.

---

## Interface

```csharp
public interface IMenuService
{
    void RegisterMenu(MenuContribution pMenu);
    void RegisterMenuItem(string pMenuId, MenuItemContribution pItem);
    void UnregisterMenu(string pMenuId);
    void UnregisterMenuItem(string pMenuId, string pItemId);
}
```

---

## How to obtain

```csharp
IMenuService lMenuService = pServiceManager.RequestService<IMenuService>();
```

---

## MenuContribution — top-level menu

```csharp
public sealed class MenuContribution
{
    public MenuContribution(string pId, string pHeader, int pOrder = 100)
}

public string Id { get; }      // stable unique key, e.g. "file", "edit", "view"
public string Header { get; }  // label shown in menu bar
public int Order { get; }      // ascending — lower = further left
```

### Header mnemonics

Prefix a letter with `_` to define a keyboard mnemonic (Alt+letter on Windows):

```csharp
new MenuContribution("file", "_File", 100)   // Alt+F
new MenuContribution("edit", "_Edit", 200)   // Alt+E
new MenuContribution("view", "_View", 300)   // Alt+V
```

---

## MenuItemContribution — menu item

```csharp
public class MenuItemContribution
{
    public MenuItemContribution(
        string pId,
        string pHeader,
        ICommand? pCommand = null,
        int pOrder = 100,
        string? pInputGesture = null,
        string? pIconUri = null)
}

public string Id { get; }
public string Header { get; }
public ICommand? Command { get; }
public int Order { get; }
public string? InputGesture { get; }   // e.g. "Ctrl+N", "F5"
public string? IconUri { get; }        // avares:// URI or absolute path
```

---

## MenuSeparatorContribution — separator line

```csharp
public sealed class MenuSeparatorContribution : MenuItemContribution
{
    public MenuSeparatorContribution(string pId, int pOrder = 100)
}
```

---

## Example — full menu with items and separator

```csharp
public void Initialize(IPluginServiceManager pServiceManager)
{
    IMenuService lMenuService = pServiceManager.RequestService<IMenuService>();

    // Register the top-level menu
    lMenuService.RegisterMenu(new MenuContribution(
        pId: "tools",
        pHeader: "_Tools",
        pOrder: 700));

    // First item — with keyboard shortcut
    lMenuService.RegisterMenuItem("tools", new MenuItemContribution(
        pId: "tools.run",
        pHeader: "_Run",
        pCommand: new RelayCommand(OnRun),
        pInputGesture: "F5",
        pOrder: 100));

    // Second item — with icon
    lMenuService.RegisterMenuItem("tools", new MenuItemContribution(
        pId: "tools.settings",
        pHeader: "_Settings…",
        pCommand: new RelayCommand(OnSettings),
        pIconUri: "avares://MyPlugin/Assets/settings.png",
        pOrder: 200));

    // Separator between groups
    lMenuService.RegisterMenuItem("tools", new MenuSeparatorContribution(
        pId: "tools.sep1",
        pOrder: 300));

    // Third item — below the separator
    lMenuService.RegisterMenuItem("tools", new MenuItemContribution(
        pId: "tools.about",
        pHeader: "_About…",
        pCommand: new RelayCommand(OnAbout),
        pOrder: 400));
}
```

Result in the menu bar:

```
Tools
├── Run          F5
├── Settings…
├── ───────────
└── About…
```

---

## Example — contributing to an existing menu

Multiple plugins can add items to the same menu. Items are sorted by `Order` regardless of registration order:

```csharp
// Plugin A registers the "tools" menu at order 700
lMenuService.RegisterMenu(new MenuContribution("tools", "_Tools", 700));
lMenuService.RegisterMenuItem("tools", new MenuItemContribution("tools.run", "_Run", ..., pOrder: 100));

// Plugin B adds its own item to the same "tools" menu
lMenuService.RegisterMenuItem("tools", new MenuItemContribution("tools.lint", "_Lint Code", ..., pOrder: 150));
```

---

## Example — CanExecute (enabling/disabling)

Use `RelayCommand` with a `canExecute` predicate:

```csharp
private bool mIsRunning = false;

RelayCommand lRunCommand = new RelayCommand(
    execute: OnRun,
    canExecute: () => !mIsRunning);

// After state changes, notify the command to re-evaluate CanExecute:
lRunCommand.NotifyCanExecuteChanged();
```

---

## Example — unregistering at runtime

```csharp
// Remove a specific item
lMenuService.UnregisterMenuItem("tools", "tools.run");

// Remove the entire menu (and all its items)
lMenuService.UnregisterMenu("tools");
```

---

## Ordering reference

| Order value | Position |
|---|---|
| 100 | First (leftmost / topmost) |
| 200, 300… | Subsequent items |
| 900 | Near last |

The shell's built-in menus use order values 100–800. Use 900+ for plugin menus that should appear at the right end of the menu bar.
