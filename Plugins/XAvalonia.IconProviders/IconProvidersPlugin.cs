using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Shell.Abstractions.Icons;
using XAvalonia.Shell.Abstractions.Plugins;

namespace XAvalonia.IconProviders;

/// <summary>
/// Plugin that registers a <see cref="MaterialIconProvider"/> with the shell's <see cref="IIconManager"/>.
/// </summary>
public sealed class IconProvidersPlugin : IPlugin
{
    /// <inheritdoc/>
    public string Id => "org.avaloniaui.shell.icon-providers";

    /// <inheritdoc/>
    public string Name => "Icon Providers";

    /// <inheritdoc/>
    public string Description => "Registers a Material Design icon provider with IIconManager.";

    /// <inheritdoc/>
    public Version CurrentVersion => new Version(1, 0, 0);

    /// <inheritdoc/>
    public string? NuGetPackageId => null;

    /// <inheritdoc/>
    public IReadOnlyList<Type> ProvidedServices => Array.Empty<Type>();

    /// <inheritdoc/>
    public void RegisterServices(IServiceCollection pServices) { }

    /// <inheritdoc/>
    public void Initialize(IPluginServiceManager pServiceManager)
    {
        IIconManager lIconManager = pServiceManager.RequestService<IIconManager>();
        lIconManager.RegisterProvider(new MaterialIconProvider());
    }
}
