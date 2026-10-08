using Microsoft.Extensions.DependencyInjection;

namespace XAvalonia.Shell.Abstractions.Plugins;

/// <summary>
/// Contract that every shell plugin must implement.
/// Initialization is split into two phases to match the DI container lifecycle:
/// <list type="number">
///   <item><see cref="RegisterServices"/> — called before the container is built; use it to register types.</item>
///   <item><see cref="Initialize"/> — called after the container is built; use it to interact with shell services.</item>
/// </list>
/// </summary>
public interface IPlugin
{
    /// <summary>Unique, reverse-DNS identifier (e.g. <c>org.mycompany.myplugin</c>).</summary>
    string Id { get; }

    /// <summary>Human-readable name shown in the plugin browser.</summary>
    string Name { get; }

    /// <summary>Short description of what this plugin does.</summary>
    string Description { get; }

    /// <summary>Version currently installed.</summary>
    Version CurrentVersion { get; }

    /// <summary>
    /// NuGet package identifier used to fetch the latest available version.
    /// <c>null</c> disables update checking for this plugin.
    /// </summary>
    string? NuGetPackageId { get; }

    /// <summary>
    /// Services (interfaces) this plugin registers into the DI container.
    /// Declared explicitly so the plugin browser can display them.
    /// </summary>
    IReadOnlyList<Type> ProvidedServices { get; }

    /// <summary>
    /// Phase 1 — Register types into the DI container.
    /// Called before <see cref="IServiceProvider"/> is built.
    /// </summary>
    /// <param name="pServices">The application service collection.</param>
    void RegisterServices(IServiceCollection pServices);

    /// <summary>
    /// Phase 2 — Interact with shell services (menus, status bar, etc.).
    /// Called after the DI container is built and Avalonia is initialized.
    /// Use <see cref="IPluginServiceManager.RequestService{T}"/> to obtain and declare each dependency.
    /// </summary>
    /// <param name="pServiceManager">Resolves shell services and tracks consumed dependencies.</param>
    void Initialize(IPluginServiceManager pServiceManager);
}
