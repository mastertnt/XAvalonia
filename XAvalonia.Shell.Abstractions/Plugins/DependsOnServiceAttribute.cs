namespace XAvalonia.Shell.Abstractions.Plugins;

/// <summary>
/// Declares that a plugin must be registered and initialized after every plugin
/// listing <see cref="ServiceType"/> in its <see cref="IPlugin.ProvidedServices"/>.
/// Can be applied several times to declare several dependencies.
/// </summary>
/// <remarks>
/// A service that no plugin provides is assumed to be supplied by the shell itself
/// and adds no ordering constraint.
/// </remarks>
/// <example>
/// <code>
/// [DependsOnService(typeof(ILogService))]
/// public sealed class MyPlugin : IPlugin { … }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class DependsOnServiceAttribute : Attribute
{
    /// <param name="pServiceType">Service interface this plugin consumes.</param>
    public DependsOnServiceAttribute(Type pServiceType)
    {
        ServiceType = pServiceType;
    }

    /// <summary>Service interface this plugin consumes.</summary>
    public Type ServiceType { get; }
}
