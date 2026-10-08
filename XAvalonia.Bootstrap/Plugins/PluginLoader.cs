using System.Diagnostics;
using System.Reflection;
using XAvalonia.Shell.Abstractions.Plugins;

namespace XAvalonia.Bootstrap.Plugins;

/// <summary>
/// Discovers and instantiates <see cref="IPlugin"/> implementations from a directory.
/// </summary>
public static class PluginLoader
{
    /// <summary>
    /// Scans <paramref name="pDirectory"/> for plugin assemblies.
    /// Returns an empty list when the directory does not exist.
    /// </summary>
    /// <param name="pDirectory">Absolute path of the plugins directory.</param>
    /// <returns>All plugin instances found, in file-system order.</returns>
    public static IReadOnlyList<IPlugin> LoadFromDirectory(string pDirectory)
    {
        if (!Directory.Exists(pDirectory))
        {
            Trace.WriteLine($"[PluginLoader] Plugins directory not found: '{pDirectory}'. No plugins loaded.");
            return Array.Empty<IPlugin>();
        }

        List<IPlugin> lPlugins = new List<IPlugin>();
        HashSet<string> lSeenFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string lFile in Directory.EnumerateFiles(pDirectory, "*.dll", SearchOption.AllDirectories))
        {
            string lFileName = Path.GetFileName(lFile);
            if (!lSeenFileNames.Add(lFileName))
            {
                Trace.WriteLine($"[PluginLoader] Skipping duplicate '{lFileName}' at '{lFile}'.");
                continue;
            }

            IReadOnlyList<IPlugin> lFound = LoadFromAssemblyFile(lFile);
            lPlugins.AddRange(lFound);
        }

        Trace.WriteLine($"[PluginLoader] Loaded {lPlugins.Count} plugin(s) from '{pDirectory}'.");
        return lPlugins;
    }

    private static IReadOnlyList<IPlugin> LoadFromAssemblyFile(string pFilePath)
    {
        List<IPlugin> lPlugins = new List<IPlugin>();

        try
        {
            Assembly lAssembly = Assembly.LoadFrom(pFilePath);

            foreach (Type lType in lAssembly.GetExportedTypes())
            {
                if (!typeof(IPlugin).IsAssignableFrom(lType) || lType.IsAbstract || lType.IsInterface)
                {
                    continue;
                }

                if (lType.GetConstructor(Type.EmptyTypes) is null)
                {
                    Trace.WriteLine($"[PluginLoader] Skipping '{lType.FullName}': no public parameter-less constructor.");
                    continue;
                }

                IPlugin lPlugin = (IPlugin)Activator.CreateInstance(lType)!;
                lPlugins.Add(lPlugin);
                Trace.WriteLine($"[PluginLoader] Found plugin '{lPlugin.Name}' ({lPlugin.Id}) v{lPlugin.CurrentVersion}.");
            }
        }
        catch (Exception lEx)
        {
            Trace.WriteLine($"[PluginLoader] Failed to load assembly '{pFilePath}': {lEx.Message}");
        }

        return lPlugins;
    }
}
