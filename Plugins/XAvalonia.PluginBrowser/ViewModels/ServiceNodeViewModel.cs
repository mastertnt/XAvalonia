using Avalonia.Media;

namespace PluginBrowser.ViewModels;

/// <summary>Represents a single service dependency node in the plugin tree.</summary>
public sealed class ServiceNodeViewModel
{
    /// <summary>
    /// Initializes the node with the service type name and its relationship to the plugin.
    /// </summary>
    /// <param name="pTypeName">Short name of the service interface (e.g. <c>IMenuService</c>).</param>
    /// <param name="pKind">Whether the service is consumed or provided by the plugin.</param>
    public ServiceNodeViewModel(string pTypeName, ServiceKind pKind)
    {
        TypeName = pTypeName;
        Kind = pKind;
        KindLabel = pKind == ServiceKind.Consumed ? "Consumed" : "Provided";
        IconColor = pKind == ServiceKind.Provided
            ? new SolidColorBrush(Color.Parse("#4CAF50"))
            : new SolidColorBrush(Color.Parse("#2196F3"));
    }

    /// <summary>Short name of the service interface.</summary>
    public string TypeName { get; }

    /// <summary>Whether the service is consumed or provided by the plugin.</summary>
    public ServiceKind Kind { get; }

    /// <summary>Human-readable label for <see cref="Kind"/>.</summary>
    public string KindLabel { get; }

    /// <summary>Icon foreground color: green for provided services, blue for consumed ones.</summary>
    public IBrush IconColor { get; }
}
