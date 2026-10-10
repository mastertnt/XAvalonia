# How to Develop a Plugin

This guide walks you through creating a plugin for XAvalonia from scratch — project setup, service registration, UI contribution, and packaging.

---

## Overview

Every plugin is a standalone class library that implements `IPlugin`. The shell discovers and loads plugins at startup by scanning the directory configured in `shell.json` recursively for `*.dll` files.

### Two-phase initialization

| Phase | Method | When | Purpose |
|---|---|---|---|
| 1 | `RegisterServices` | Before DI container is built | Register your types into the DI container |
| 2 | `Initialize` | After DI container is built | Request shell services, contribute UI |

---

## Step 1 — Create the project

Create a new **Class Library** targeting `net8.0`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <AssemblyName>MyCompany.MyPlugin</AssemblyName>
    <RootNamespace>MyCompany.MyPlugin</RootNamespace>
    <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Avalonia"                  Version="11.3.11" />
    <PackageReference Include="Avalonia.Themes.Fluent"    Version="11.3.11" />
    <PackageReference Include="Dock.Avalonia"             Version="11.3.11.22" />
    <PackageReference Include="Dock.Model.ReactiveUI"     Version="11.3.11.22" />
    <PackageReference Include="CommunityToolkit.Mvvm"     Version="8.4.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\XAvalonia.Shell.Abstractions\XAvalonia.Shell.Abstractions.csproj" />
  </ItemGroup>

</Project>
```

> Only reference `XAvalonia.Shell.Abstractions` — never `XAvalonia.Bootstrap` or other plugins. Keep your plugin fully decoupled from the host.

---

## Step 2 — Implement IPlugin

Create the plugin entry point. One class per plugin, one file per class:

```csharp
using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Shell.Abstractions.Plugins;

namespace MyCompany.MyPlugin;

public sealed class MyPlugin : IPlugin
{
    // Unique reverse-DNS identifier — never change this after shipping
    public string Id => "com.mycompany.myplugin";

    public string Name => "My Plugin";
    public string Description => "Does something useful.";
    public Version CurrentVersion => new Version(1, 0, 0);

    // Set to your NuGet package ID to enable automatic update checks,
    // or null for internal / built-in plugins
    public string? NuGetPackageId => "MyCompany.MyPlugin";

    // Declare every interface YOUR plugin registers into DI here
    public IReadOnlyList<Type> ProvidedServices => Array.Empty<Type>();

    // Phase 1 — register your own services before the container is built
    public void RegisterServices(IServiceCollection pServices)
    {
        // Example: pServices.AddSingleton<IMyService, MyService>();
    }

    // Phase 2 — request shell services and contribute UI
    public void Initialize(IPluginServiceManager pServiceManager)
    {
        // Always try to get the logger first — it may not exist
        ILogService? lLog = pServiceManager.TryRequestService<ILogService>();
        lLog?.LogInfo($"[{Name}] Initializing…");

        // … contribute menus, panels, documents here …

        lLog?.LogInfo($"[{Name}] Ready.");
    }
}
```

### Key rules for `Id`
- Must be globally unique — use reverse-DNS (`com.mycompany.myplugin`)
- Never change it after shipping — it is used as a stable key by the shell

---

## Step 3 — Request services

Inside `Initialize`, use `IPluginServiceManager` to access shell services:

```csharp
// Required service — throws if not found
IMenuService lMenuService = pServiceManager.RequestService<IMenuService>();

// Optional service — returns null if not registered
ILogService? lLog = pServiceManager.TryRequestService<ILogService>();
```

Every resolved service is automatically recorded and shown in the Plugin Browser under "Consumed services".

### Available shell services

| Interface | Package | Purpose |
|---|---|---|
| `IMenuService` | Shell.Abstractions | Add top-level menus and items |
| `IDocumentService` | Shell.Abstractions | Open/close document tabs |
| `IToolPanelService` | Shell.Abstractions | Add docked tool panels |
| `IStatusBarService` | Shell.Abstractions | Add status bar items |
| `IMainWindowService` | Shell.Abstractions | Control window title, icon, lifecycle |
| `IPluginManager` | Shell.Abstractions | Enumerate plugins, check updates |
| `ILogService` | Shell.Abstractions | Emit and receive log messages |

---

## Step 4 — Add a menu

```csharp
IMenuService lMenuService = pServiceManager.RequestService<IMenuService>();

// Register the top-level menu (only once — use a stable ID)
lMenuService.RegisterMenu(new MenuContribution(
    pId: "mymenu",
    pHeader: "_My Menu",
    pOrder: 500));            // lower = further left

// Register a menu item inside it
lMenuService.RegisterMenuItem("mymenu", new MenuItemContribution(
    pId: "mymenu.hello",
    pHeader: "_Say Hello",
    pCommand: new RelayCommand(SayHello),
    pInputGesture: "Ctrl+H",
    pOrder: 100));

// Register a separator
lMenuService.RegisterMenuItem("mymenu", new MenuSeparatorContribution(
    pId: "mymenu.sep1",
    pOrder: 200));
```

---

## Step 5 — Open a document tab

A document is a central tab in the dock area. Its ViewModel must extend `Document` from `Dock.Model.ReactiveUI`.

**ViewModel:**

```csharp
using Dock.Model.ReactiveUI.Controls;

namespace MyCompany.MyPlugin.ViewModels;

public sealed class HelloDocumentViewModel : Document
{
    public HelloDocumentViewModel()
    {
        Id    = "hello-doc";      // must be unique and stable
        Title = "Hello";
        CanClose = true;
    }

    public string Greeting => "Hello from my plugin!";
}
```

**View** (`HelloDocumentView.axaml` — convention: replace "ViewModel" with "View"):

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:MyCompany.MyPlugin.ViewModels"
             x:Class="MyCompany.MyPlugin.Views.HelloDocumentView"
             x:DataType="vm:HelloDocumentViewModel">

  <TextBlock Text="{Binding Greeting}"
             HorizontalAlignment="Center"
             VerticalAlignment="Center"
             FontSize="24" />
</UserControl>
```

**View code-behind:**

```csharp
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MyCompany.MyPlugin.Views;

public partial class HelloDocumentView : UserControl
{
    public HelloDocumentView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
```

**Opening the document from the plugin:**

```csharp
IDocumentService lDocService = pServiceManager.RequestService<IDocumentService>();

lDocService.RegisterDocument(new DocumentContribution(
    pId: "hello-doc",
    pTitle: "Hello",
    pViewModel: new HelloDocumentViewModel()));
```

> The `Id` in `DocumentContribution` must exactly match the ViewModel's `Id` property.

---

## Step 6 — Add a tool panel

A tool panel is a dockable side/bottom panel. Its ViewModel must extend `Tool` from `Dock.Model.ReactiveUI`.

**ViewModel:**

```csharp
using Dock.Model.ReactiveUI.Controls;

namespace MyCompany.MyPlugin.ViewModels;

public sealed class MyToolViewModel : Tool
{
    public MyToolViewModel()
    {
        Id    = "my-tool";
        Title = "My Tool";
        CanClose = true;
        CanPin   = true;
        CanFloat = true;
    }
}
```

**View** (`MyToolView.axaml`):

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:MyCompany.MyPlugin.ViewModels"
             x:Class="MyCompany.MyPlugin.Views.MyToolView"
             x:DataType="vm:MyToolViewModel">

  <TextBlock Text="My Tool Panel"
             Margin="8"
             Classes="panelHeader" />
</UserControl>
```

**Registering the panel:**

```csharp
IToolPanelService lToolPanelService = pServiceManager.RequestService<IToolPanelService>();

lToolPanelService.RegisterPanel(new ToolPanelContribution(
    pId: "my-tool",
    pTitle: "My Tool",
    pViewModel: new MyToolViewModel(),
    pAlignment: ToolPanelAlignment.Left));   // Left | Right | Bottom | Top
```

---

## Step 7 — Add a status bar item

```csharp
IStatusBarService lStatusBar = pServiceManager.RequestService<IStatusBarService>();

lStatusBar.RegisterItem(new StatusBarItemContribution(
    pId: "myplugin.status",
    pText: "My Plugin ready",
    pAlignment: StatusBarItemAlignment.Left,
    pOrder: 200));

// Update later from any thread:
lStatusBar.UpdateItem("myplugin.status", "My Plugin: processing…");
```

---

## Step 8 — Provide your own service

If your plugin exposes a service that other plugins can consume:

```csharp
// 1. Declare the interface in XAvalonia.Shell.Abstractions (or your own shared project)
public interface IMyService
{
    void DoSomething();
}

// 2. Implement it
public sealed class MyService : IMyService
{
    public void DoSomething() { /* … */ }
}

// 3. Register in Phase 1
public void RegisterServices(IServiceCollection pServices)
{
    pServices.AddSingleton<IMyService, MyService>();
}

// 4. Declare in ProvidedServices
public IReadOnlyList<Type> ProvidedServices => new[] { typeof(IMyService) };
```

Other plugins can then call `pServiceManager.RequestService<IMyService>()`.

### Loading order

Plugins are registered (Phase 1) and initialized (Phase 2) in dependency order. Declare what your plugin needs on its class:

```csharp
// Initialize after every plugin that lists IMyService in its ProvidedServices.
// A service no plugin provides (e.g. a shell service) adds no constraint.
[DependsOnService(typeof(IMyService))]

// Initialize after the plugin with this Id. Startup fails if it is not installed…
[DependsOnPlugin("com.mycompany.core")]

// …unless the dependency is optional.
[DependsOnPlugin("com.mycompany.extras", Optional = true)]
public sealed class MyConsumerPlugin : IPlugin { … }
```

Plugins with no constraint between them keep their discovery order (assembly paths sorted ordinally). Startup throws a `PluginDependencyException` when two plugins share an `Id`, a required plugin is missing, or the dependencies form a cycle (the message shows the cycle, e.g. `'a' -> 'b' -> 'a'`).

---

## Step 9 — View resolution (naming convention)

The shell's `ViewLocator` resolves views by replacing `ViewModel` with `View` in the full type name:

| ViewModel type | Resolved view type |
|---|---|
| `MyCompany.MyPlugin.ViewModels.HelloDocumentViewModel` | `MyCompany.MyPlugin.Views.HelloDocumentView` |
| `MyCompany.MyPlugin.ViewModels.MyToolViewModel` | `MyCompany.MyPlugin.Views.MyToolView` |

The view must be a public class with a public parameterless constructor. The namespace does not need to match exactly — only the type name suffix (`View` replacing `ViewModel`) matters.

---

## Step 10 — Configure the shell to find your plugin

In `shell.json`, point `pluginsDirectory` to a path that contains your plugin's `bin/` output:

```json
{
  "pluginsDirectory": "D:\\Projects\\MyApp"
}
```

Scanning is recursive — the shell will find your DLL anywhere under that directory.

---

## Complete minimal example

```
MyCompany.MyPlugin/
├── MyCompany.MyPlugin.csproj
├── MyPlugin.cs                          ← IPlugin entry point
├── ViewModels/
│   └── HelloDocumentViewModel.cs        ← extends Document
└── Views/
    ├── HelloDocumentView.axaml
    └── HelloDocumentView.axaml.cs
```

**`MyPlugin.cs`:**

```csharp
public sealed class MyPlugin : IPlugin
{
    private IDocumentService? mDocumentService;

    public string Id => "com.mycompany.myplugin";
    public string Name => "My Plugin";
    public string Description => "Opens a Hello document.";
    public Version CurrentVersion => new Version(1, 0, 0);
    public string? NuGetPackageId => null;
    public IReadOnlyList<Type> ProvidedServices => Array.Empty<Type>();

    public void RegisterServices(IServiceCollection pServices) { }

    public void Initialize(IPluginServiceManager pServiceManager)
    {
        IMenuService lMenuService = pServiceManager.RequestService<IMenuService>();
        mDocumentService = pServiceManager.RequestService<IDocumentService>();

        lMenuService.RegisterMenu(new MenuContribution("hello", "_Hello", 900));
        lMenuService.RegisterMenuItem("hello", new MenuItemContribution(
            pId: "hello.open",
            pHeader: "_Open Hello…",
            pCommand: new RelayCommand(OpenHello)));
    }

    private void OpenHello()
    {
        if (mDocumentService!.IsDocumentOpen("hello-doc"))
        {
            mDocumentService.ActivateDocument("hello-doc");
            return;
        }

        mDocumentService.RegisterDocument(new DocumentContribution(
            pId: "hello-doc",
            pTitle: "Hello",
            pViewModel: new HelloDocumentViewModel()));
    }
}
```

---

## Checklist

- [ ] `Id` is a unique reverse-DNS string
- [ ] `ProvidedServices` lists every interface registered by `RegisterServices`
- [ ] Services are only requested in `Initialize`, never in `RegisterServices`
- [ ] ViewModel `Id` matches the `DocumentContribution`/`ToolPanelContribution` `pId`
- [ ] View type name = ViewModel type name with `ViewModel` → `View`
- [ ] No reference to `XAvalonia.Bootstrap` or other plugins
- [ ] `ILogService` is obtained via `TryRequestService` (it may not exist)
