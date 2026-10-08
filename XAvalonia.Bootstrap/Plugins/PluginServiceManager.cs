using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Shell.Abstractions.Plugins;

namespace XAvalonia.Bootstrap.Plugins;

/// <summary>
/// Per-plugin implementation of <see cref="IPluginServiceManager"/>.
/// Resolves services from the DI container and records each requested type
/// so consumed dependencies are tracked automatically.
/// </summary>
public sealed class PluginServiceManager : IPluginServiceManager
{
    private readonly IServiceProvider mServiceProvider;
    private readonly List<Type> mConsumedServices = new List<Type>();

    /// <summary>Initializes the manager with the DI container to resolve services from.</summary>
    /// <param name="pServiceProvider">The application service provider.</param>
    public PluginServiceManager(IServiceProvider pServiceProvider)
    {
        mServiceProvider = pServiceProvider;
    }

    /// <inheritdoc/>
    public T RequestService<T>() where T : notnull
    {
        Type lType = typeof(T);
        if (!mConsumedServices.Contains(lType))
        {
            mConsumedServices.Add(lType);
        }
        return mServiceProvider.GetRequiredService<T>();
    }

    /// <inheritdoc/>
    public T? TryRequestService<T>() where T : class
    {
        T? lService = mServiceProvider.GetService<T>();
        if (lService is not null)
        {
            Type lType = typeof(T);
            if (!mConsumedServices.Contains(lType))
            {
                mConsumedServices.Add(lType);
            }
        }
        return lService;
    }

    /// <summary>All service types requested by the plugin so far, in request order.</summary>
    public IReadOnlyList<Type> ConsumedServices => mConsumedServices;
}
