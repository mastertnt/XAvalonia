namespace XAvalonia.Shell.Abstractions.Plugins;

/// <summary>
/// Provides a plugin with access to shell services during <see cref="IPlugin.Initialize"/>.
/// Each call to <see cref="RequestService{T}"/> both resolves the service and records it as
/// consumed, so the plugin browser can display dependencies without manual declaration.
/// </summary>
public interface IPluginServiceManager
{
    /// <summary>
    /// Resolves service <typeparamref name="T"/> from the shell container and records it
    /// as consumed by this plugin.
    /// </summary>
    /// <typeparam name="T">The service interface to resolve.</typeparam>
    /// <returns>The resolved service instance.</returns>
    T RequestService<T>() where T : notnull;

    /// <summary>
    /// Tries to resolve optional service <typeparamref name="T"/> from the shell container.
    /// Returns <c>null</c> if the service is not registered; does not throw.
    /// The service is recorded as consumed only when it is found.
    /// </summary>
    /// <typeparam name="T">The service interface to resolve.</typeparam>
    /// <returns>The resolved service instance, or <c>null</c> if not available.</returns>
    T? TryRequestService<T>() where T : class;
}
