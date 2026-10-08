using System;

namespace XAvalonia.Shell.Abstractions.Plugins;

/// <summary>
/// Exposes the list of all loaded plugins, their consumed-service records, and NuGet update checking.
/// </summary>
public interface IPluginManager
{
    /// <summary>
    /// Fired once, immediately after every plugin's <see cref="IPlugin.Initialize"/> has completed.
    /// </summary>
    event EventHandler? AllPluginsLoaded;

    /// <summary>All plugins discovered and loaded at startup, in load order.</summary>
    IReadOnlyList<IPlugin> Plugins { get; }

    /// <summary>
    /// Returns the service types consumed by the plugin with the given ID,
    /// as recorded during <see cref="IPlugin.Initialize"/>.
    /// Returns an empty list if the ID is unknown or the plugin has not been initialized yet.
    /// </summary>
    /// <param name="pPluginId">The unique plugin identifier.</param>
    IReadOnlyList<Type> GetConsumedServices(string pPluginId);

    /// <summary>
    /// Fetches the latest stable version of a NuGet package from the NuGet v3 API.
    /// Results are cached for the lifetime of the application.
    /// </summary>
    /// <param name="pNuGetPackageId">Case-insensitive NuGet package identifier.</param>
    /// <param name="pCancellationToken">Token to cancel the HTTP request.</param>
    /// <returns>
    /// The latest stable <see cref="Version"/>, or <c>null</c> if the package is not found
    /// or the request fails.
    /// </returns>
    Task<Version?> GetAvailableVersionAsync(string pNuGetPackageId, CancellationToken pCancellationToken = default);
}
