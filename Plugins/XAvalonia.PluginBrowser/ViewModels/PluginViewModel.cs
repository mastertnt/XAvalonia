using CommunityToolkit.Mvvm.ComponentModel;
using XAvalonia.Shell.Abstractions.Plugins;

namespace PluginBrowser.ViewModels;

/// <summary>View model for a single plugin node in the plugin browser tree.</summary>
public partial class PluginViewModel : ObservableObject
{
    // [ObservableProperty] fields use _ prefix: required by the CommunityToolkit.Mvvm
    // source generator to correctly derive the property name (e.g. _availableVersion -> AvailableVersion).
    [ObservableProperty]
    private string _availableVersion = "…";

    /// <summary>Initializes the view model from an <see cref="IPlugin"/> instance.</summary>
    /// <param name="pPlugin">The plugin to display.</param>
    /// <param name="pConsumedServices">Services consumed by the plugin, as recorded during initialization.</param>
    public PluginViewModel(IPlugin pPlugin, IReadOnlyList<Type> pConsumedServices)
    {
        Id = pPlugin.Id;
        Name = pPlugin.Name;
        Description = pPlugin.Description;
        CurrentVersion = pPlugin.CurrentVersion.ToString();
        NuGetPackageId = pPlugin.NuGetPackageId;
        AssemblyLocation = pPlugin.GetType().Assembly.Location;

        List<ServiceNodeViewModel> lNodes = new List<ServiceNodeViewModel>();

        foreach (Type lType in pPlugin.ProvidedServices)
        {
            lNodes.Add(new ServiceNodeViewModel(lType.Name, ServiceKind.Provided));
        }

        foreach (Type lType in pConsumedServices)
        {
            lNodes.Add(new ServiceNodeViewModel(lType.Name, ServiceKind.Consumed));
        }

        ServiceNodes = lNodes;
    }

    /// <summary>Unique plugin identifier.</summary>
    public string Id { get; }

    /// <summary>Human-readable name.</summary>
    public string Name { get; }

    /// <summary>Short description of the plugin's purpose.</summary>
    public string Description { get; }

    /// <summary>Version currently installed.</summary>
    public string CurrentVersion { get; }

    /// <summary>NuGet package ID used to fetch the latest available version, or <c>null</c>.</summary>
    public string? NuGetPackageId { get; }

    /// <summary>Full path to the assembly file on disk.</summary>
    public string AssemblyLocation { get; }

    /// <summary>Child nodes representing the services provided and consumed by this plugin.</summary>
    public IReadOnlyList<ServiceNodeViewModel> ServiceNodes { get; }
}
