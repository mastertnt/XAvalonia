# XAvalonia

> A plugin-based, extensible desktop shell framework built on [Avalonia UI](https://avaloniaui.net/) — inspired by [Gemini](https://github.com/tgjones/gemini) for WPF, reimagined for cross-platform .NET.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Avalonia 11](https://img.shields.io/badge/Avalonia-11.3-blueviolet?logo=avalonia)](https://avaloniaui.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)]()

---

<!-- Replace with an actual screenshot -->
![XAvalonia Screenshot](Docs/screenshot.png)

---

## What is XAvalonia?

XAvalonia is a **shell framework** for building extensible desktop applications on top of Avalonia UI.
It provides the plumbing — docking layout, menus, status bar, panels, documents — so that each feature
of your application lives in an isolated, self-describing **plugin**.

The architecture is directly inspired by the Eclipse/Gemini model:

- The **shell** owns the window, the dock, and the service bus.
- **Plugins** discover each other at runtime, register services, and contribute UI via a clean contribution API.
- Your `Program.cs` is a single line.

```csharp
[STAThread]
public static void Main(string[] pArgs) => ShellApplication.Run(pArgs);
```

---

## Features

| | |
|---|---|
| 🧩 **Plugin system** | Runtime assembly scanning, two-phase lifecycle, automatic dependency tracking |
| 🪟 **Docking layout** | Full dock/float/split via [Dock.Avalonia](https://github.com/wieslawsoltes/Dock) |
| 📄 **Document tabs** | Open, activate, and close tabbed documents from any plugin |
| 🔧 **Tool panels** | Register Left / Right / Bottom / Top panels dynamically |
| 📋 **Menu contributions** | Add top-level menus and items with commands, icons, and keyboard shortcuts |
| 📊 **Status bar** | Left/right aligned items, updated at runtime |
| ⚙️ **Configuration** | Declarative JSON binding via `[TechConfiguration]` / `[UserConfiguration]` attributes |
| 🔌 **NuGet update checks** | Plugins can expose a `NuGetPackageId` for live update detection |
| 📦 **DI built-in** | `Microsoft.Extensions.DependencyInjection` throughout |
| 🎨 **Fluent theme** | Avalonia Fluent Dark theme out of the box |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        XAvalonia.SampleApp                      │
│                     (entry point — 1 line)                      │
└──────────────────────────┬──────────────────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────────────────┐
│                      XAvalonia.Bootstrap                        │
│   ShellApplication · PluginLoader · PluginManager · Services    │
└──────────────────────────┬──────────────────────────────────────┘
                           │ depends on
┌──────────────────────────▼──────────────────────────────────────┐
│               XAvalonia.Shell.Abstractions                      │
│   IPlugin · IMenuService · IDocumentService · IToolPanelService │
│   IStatusBarService · IMainWindowService · ILogService …        │
└─────────────────────────────────────────────────────────────────┘
                           ▲
              ┌────────────┼────────────┐
              │            │            │
    ┌─────────┴──┐  ┌──────┴─────┐  ┌──┴──────────┐
    │ MainWindow │  │PluginBrowser│  │  YourPlugin  │
    │  Plugin    │  │   Plugin    │  │   (future)   │
    └────────────┘  └────────────┘  └─────────────-┘
```

**Plugins only ever reference `XAvalonia.Shell.Abstractions`** — zero coupling to the framework internals.

### Default layout

```
┌─────────────────────────────────────────────────────┐
│  File   View   Help                                 │  ← Menu bar
├──────────┬──────────────────────────┬───────────────┤
│          │  [ Doc 1 ] [ Doc 2 ] …  │               │
│          ├──────────────────────────┤               │
│ Explorer │   Document content…      │  Properties   │
│          ├──────────────────────────┤               │
│          │  Output                  │               │
│          │  [INFO] Application …    │               │
├──────────┴──────────────────────────┴───────────────┤
│  Ready          No file open                  Ln 1  │  ← Status bar
└─────────────────────────────────────────────────────┘
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Run the sample app

```bash
git clone https://github.com/your-org/xavalonia.git
cd xavalonia
dotnet run --project XAvalonia.SampleApp
```

---

## Writing a Plugin

A plugin is a class library that references only `XAvalonia.Shell.Abstractions`.

### 1. Implement `IPlugin`

```csharp
public sealed class HelloPlugin : IPlugin
{
    public string Id          => "com.example.hello";
    public string Name        => "Hello Plugin";
    public string Description => "A minimal XAvalonia plugin.";
    public Version CurrentVersion    => new(1, 0, 0);
    public string? NuGetPackageId    => null;
    public IReadOnlyList<Type> ProvidedServices => [];

    public void RegisterServices(IServiceCollection pServices)
    {
        pServices.AddSingleton<HelloDocumentViewModel>();
    }

    public void Initialize(IPluginServiceManager pServiceManager)
    {
        IMenuService lMenus = pServiceManager.RequestService<IMenuService>();
        IDocumentService lDocs = pServiceManager.RequestService<IDocumentService>();

        lMenus.RegisterMenuItem("menu.file", new MenuItemContribution
        {
            Id      = "menu.file.hello",
            Header  = "_Hello World",
            Order   = 100,
            Command = ReactiveCommand.Create(() =>
                lDocs.RegisterDocument(new DocumentContribution
                {
                    Id        = "doc.hello",
                    Title     = "Hello",
                    ViewModel = pServiceManager.RequestService<HelloDocumentViewModel>()
                }))
        });
    }
}
```

### 2. Drop the DLL into the plugins directory

Point `pluginsDirectory` in `technical_configuration.json` to the folder containing your plugin's output.
XAvalonia scans all assemblies at startup — no manual registration required.

```json
{
  "pluginsDirectory": "C:\\MyApp\\Plugins"
}
```

### 3. Follow the View convention

The **ViewLocator** resolves views by naming convention:

| ViewModel | → View |
|---|---|
| `My.Plugin.ViewModels.HelloDocumentViewModel` | `My.Plugin.Views.HelloDocumentView` |

---

## Project Structure

```
XAvalonia/
├── XAvalonia.Shell.Abstractions/   ← Plugin contract (no UI deps)
├── XAvalonia.Bootstrap/            ← Framework core & services
├── XAvalonia.MainWindow/           ← Default dock layout plugin
├── XAvalonia.SampleApp/            ← Minimal entry-point app
└── Plugins/
    ├── XAvalonia.PluginBrowser/    ← Browse & update plugins
    ├── XAvalonia.LogViewer/        ← Output panel
    └── XAvalonia.Splashscreen/     ← Startup splash screen
```

---

## Key Dependencies

| Package | Version | Role |
|---|---|---|
| [Avalonia](https://avaloniaui.net/) | 11.3 | Cross-platform UI framework |
| [Dock.Avalonia](https://github.com/wieslawsoltes/Dock) | 11.3 | Docking layout engine |
| [CommunityToolkit.Mvvm](https://aka.ms/mvvmtoolkit) | 8.4 | MVVM source generators |
| Microsoft.Extensions.DependencyInjection | 8.0 | IoC container |

---

## Roadmap

- [ ] Add a screenshot / GIF demo
- [ ] NuGet package for `XAvalonia.Shell.Abstractions`
- [ ] Theming API (light / dark / custom)
- [ ] Plugin marketplace integration
- [ ] Persistent user layout per plugin

---

## Acknowledgements

Inspired by **[Gemini](https://github.com/tgjones/gemini)** — the excellent WPF shell framework by [@tgjones](https://github.com/tgjones).
XAvalonia brings the same plugin-first philosophy to cross-platform desktop development via Avalonia UI.

---

## License

MIT — see [LICENSE](LICENSE).
