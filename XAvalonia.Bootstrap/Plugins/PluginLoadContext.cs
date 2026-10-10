using System.Reflection;
using System.Runtime.Loader;

namespace XAvalonia.Bootstrap.Plugins;

/// <summary>
/// Isolated <see cref="AssemblyLoadContext"/> holding one plugin assembly and its private dependencies.
/// Assemblies the host already provides (the plugin contracts, Avalonia, Dock, DI…) are always taken
/// from the default context so that types such as <c>IPlugin</c> keep a single identity across plugins.
/// </summary>
internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private static readonly Lazy<HashSet<string>> HostAssemblyNames = new Lazy<HashSet<string>>(ReadHostAssemblyNames);

    private readonly AssemblyDependencyResolver mResolver;
    private readonly string mPluginDirectory;

    /// <summary>Creates a context for the plugin assembly at <paramref name="pPluginPath"/>.</summary>
    /// <param name="pPluginPath">Absolute path of the plugin assembly.</param>
    public PluginLoadContext(string pPluginPath)
        : base($"Plugin:{Path.GetFileNameWithoutExtension(pPluginPath)}")
    {
        mResolver = new AssemblyDependencyResolver(pPluginPath);
        mPluginDirectory = Path.GetDirectoryName(pPluginPath)!;
    }

    /// <summary>
    /// Returns <c>true</c> when the assembly with the given simple name is provided by the host application
    /// and must therefore be shared through the default context instead of being loaded per plugin.
    /// </summary>
    /// <param name="pSimpleName">Simple assembly name (no extension, no version).</param>
    public static bool IsHostAssembly(string pSimpleName)
    {
        return HostAssemblyNames.Value.Contains(pSimpleName)
            || Default.Assemblies.Any(pA => string.Equals(pA.GetName().Name, pSimpleName, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc/>
    protected override Assembly? Load(AssemblyName pAssemblyName)
    {
        if (pAssemblyName.Name is null || IsHostAssembly(pAssemblyName.Name))
        {
            // Defer to the default context.
            return null;
        }

        string? lPath = mResolver.ResolveAssemblyToPath(pAssemblyName);
        if (lPath is null)
        {
            string lCandidate = Path.Combine(mPluginDirectory, pAssemblyName.Name + ".dll");
            lPath = File.Exists(lCandidate) ? lCandidate : null;
        }

        return lPath is null ? null : LoadFromAssemblyPath(lPath);
    }

    /// <inheritdoc/>
    protected override IntPtr LoadUnmanagedDll(string pUnmanagedDllName)
    {
        string? lPath = mResolver.ResolveUnmanagedDllToPath(pUnmanagedDllName);
        return lPath is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(lPath);
    }

    // Simple names of every assembly listed in the host's deps.json (trusted platform assemblies).
    private static HashSet<string> ReadHostAssemblyNames()
    {
        HashSet<string> lNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string lTpa)
        {
            foreach (string lPath in lTpa.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                lNames.Add(Path.GetFileNameWithoutExtension(lPath));
            }
        }

        return lNames;
    }
}
