# IPluginManager

Exposes the list of all loaded plugins, the services each one consumes, and a NuGet version-check utility. Primarily useful for plugins that want to inspect or display the plugin ecosystem (e.g., a plugin browser or an about dialog).

---

## Interface

```csharp
public interface IPluginManager
{
    event EventHandler? AllPluginsLoaded;

    IReadOnlyList<IPlugin> Plugins { get; }

    IReadOnlyList<Type> GetConsumedServices(string pPluginId);

    Task<Version?> GetAvailableVersionAsync(
        string pNuGetPackageId,
        CancellationToken pCancellationToken = default);
}
```

---

## How to obtain

```csharp
IPluginManager lPluginManager = pServiceManager.RequestService<IPluginManager>();
```

---

## Properties and members

### `Plugins`

All plugins loaded at startup, in file-system discovery order:

```csharp
IReadOnlyList<IPlugin> lPlugins = lPluginManager.Plugins;
```

Each `IPlugin` exposes:

```csharp
string Id { get; }
string Name { get; }
string Description { get; }
Version CurrentVersion { get; }
string? NuGetPackageId { get; }
IReadOnlyList<Type> ProvidedServices { get; }
```

---

### `AllPluginsLoaded`

Fires once after every plugin's `Initialize` method has been called:

```csharp
lPluginManager.AllPluginsLoaded += OnAllPluginsLoaded;

private void OnAllPluginsLoaded(object? pSender, EventArgs pArgs)
{
    // Safe to assume all services are available at this point
}
```

---

### `GetConsumedServices(string pPluginId)`

Returns the service types that a plugin resolved via `IPluginServiceManager` during `Initialize`. Tracked automatically — no extra code needed in your plugin.

```csharp
IReadOnlyList<Type> lConsumed = lPluginManager.GetConsumedServices("com.mycompany.myplugin");

foreach (Type lType in lConsumed)
{
    Console.WriteLine($"  Consumes: {lType.Name}");
}
```

---

### `GetAvailableVersionAsync`

Queries NuGet for the latest stable version of a package. Results are cached for the application lifetime:

```csharp
Version? lLatest = await lPluginManager.GetAvailableVersionAsync(
    "MyCompany.MyPlugin",
    cancellationToken);

if (lLatest is not null && lLatest > CurrentVersion)
{
    lLog?.LogInfo($"Update available: {lLatest}");
}
```

Returns `null` if:
- The plugin has no `NuGetPackageId`
- The package is not found on NuGet
- The request times out (10 s) or fails

---

## Example — enumerate all plugins

```csharp
public void Initialize(IPluginServiceManager pServiceManager)
{
    IPluginManager lPluginManager = pServiceManager.RequestService<IPluginManager>();

    foreach (IPlugin lPlugin in lPluginManager.Plugins)
    {
        IReadOnlyList<Type> lConsumed  = lPluginManager.GetConsumedServices(lPlugin.Id);
        IReadOnlyList<Type> lProvided  = lPlugin.ProvidedServices;

        Console.WriteLine($"{lPlugin.Name} v{lPlugin.CurrentVersion}");
        Console.WriteLine($"  Provides:  {string.Join(", ", lProvided.Select(t => t.Name))}");
        Console.WriteLine($"  Consumes:  {string.Join(", ", lConsumed.Select(t => t.Name))}");
    }
}
```

---

## Example — check for updates asynchronously

```csharp
private async Task CheckUpdatesAsync(
    IPluginManager pPluginManager,
    CancellationToken pToken)
{
    foreach (IPlugin lPlugin in pPluginManager.Plugins)
    {
        if (lPlugin.NuGetPackageId is null) { continue; }

        Version? lLatest = await pPluginManager.GetAvailableVersionAsync(
            lPlugin.NuGetPackageId, pToken);

        if (lLatest is null)
        {
            Console.WriteLine($"{lPlugin.Name}: unavailable");
        }
        else if (lLatest > lPlugin.CurrentVersion)
        {
            Console.WriteLine($"{lPlugin.Name}: update available ({lPlugin.CurrentVersion} → {lLatest})");
        }
        else
        {
            Console.WriteLine($"{lPlugin.Name}: up to date ({lPlugin.CurrentVersion})");
        }
    }
}
```

---

## Example — act after all plugins are loaded

`AllPluginsLoaded` is the right place to trigger work that requires the full service graph:

```csharp
public void Initialize(IPluginServiceManager pServiceManager)
{
    IPluginManager lPluginManager = pServiceManager.RequestService<IPluginManager>();

    lPluginManager.AllPluginsLoaded += (_, _) =>
    {
        // All IPlugin.Initialize() calls are done — cross-plugin wiring is safe here
        WireupCrossPluginEvents();
    };
}
```

---

## Getting the assembly path of a plugin

`IPlugin` does not expose a path property. Retrieve it via reflection:

```csharp
string lPath = plugin.GetType().Assembly.Location;
```

This is how the Plugin Browser shows assembly locations in its tooltips.
