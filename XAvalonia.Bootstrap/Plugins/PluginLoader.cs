using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using XAvalonia.Shell.Abstractions.Plugins;

namespace XAvalonia.Bootstrap.Plugins;

/// <summary>
/// Discovers and instantiates <see cref="IPlugin"/> implementations from a directory.
/// Each plugin assembly is loaded into its own <see cref="PluginLoadContext"/>; a plugin that
/// fails to load is logged and skipped so the shell keeps running.
/// </summary>
public static class PluginLoader
{
    private static readonly string ContractsAssemblyName = typeof(IPlugin).Assembly.GetName().Name!;

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

    // Loads one candidate assembly into its own PluginLoadContext and instantiates its plugins.
    // Any failure is logged and the file (or the faulty type) is skipped; nothing is rethrown.
    private static IReadOnlyList<IPlugin> LoadFromAssemblyFile(string pFilePath)
    {
        List<IPlugin> lPlugins = new List<IPlugin>();

        if (!IsPluginCandidate(pFilePath))
        {
            return lPlugins;
        }

        Type[] lTypes;
        try
        {
            PluginLoadContext lContext = new PluginLoadContext(pFilePath);
            Assembly lAssembly = lContext.LoadFromAssemblyPath(pFilePath);
            lTypes = lAssembly.GetExportedTypes();
        }
        catch (ReflectionTypeLoadException lEx)
        {
            string lDetails = string.Join("; ", lEx.LoaderExceptions.Where(pE => pE is not null).Select(pE => pE!.Message).Distinct());
            Trace.WriteLine($"[PluginLoader] Failed to load types from '{pFilePath}', plugin skipped: {lDetails}");
            return lPlugins;
        }
        catch (Exception lEx)
        {
            Trace.WriteLine($"[PluginLoader] Failed to load assembly '{pFilePath}', plugin skipped: {lEx}");
            return lPlugins;
        }

        foreach (Type lType in lTypes)
        {
            try
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
            catch (Exception lEx)
            {
                Exception lInner = lEx is TargetInvocationException { InnerException: not null } ? lEx.InnerException! : lEx;
                Trace.WriteLine($"[PluginLoader] Failed to instantiate '{lType.FullName}' from '{pFilePath}', plugin skipped: {lInner}");
            }
        }

        return lPlugins;
    }

    // A file is a plugin candidate when it is a managed assembly that is not provided by the host
    // and references the plugin contracts assembly. Read from metadata only, so that third-party
    // dependencies sitting next to the plugins are never loaded on their own.
    private static bool IsPluginCandidate(string pFilePath)
    {
        try
        {
            using FileStream lStream = File.OpenRead(pFilePath);
            using PEReader lPeReader = new PEReader(lStream);
            if (!lPeReader.HasMetadata)
            {
                return false;
            }

            MetadataReader lReader = lPeReader.GetMetadataReader();
            if (!lReader.IsAssembly)
            {
                return false;
            }

            string lName = lReader.GetString(lReader.GetAssemblyDefinition().Name);
            if (PluginLoadContext.IsHostAssembly(lName))
            {
                return false;
            }

            return lReader.AssemblyReferences
                .Select(pHandle => lReader.GetString(lReader.GetAssemblyReference(pHandle).Name))
                .Contains(ContractsAssemblyName, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception lEx)
        {
            Trace.WriteLine($"[PluginLoader] Cannot read '{pFilePath}', skipped: {lEx.Message}");
            return false;
        }
    }
}
