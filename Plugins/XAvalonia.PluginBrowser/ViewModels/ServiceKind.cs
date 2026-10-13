namespace PluginBrowser.ViewModels;

/// <summary>Indicates whether a service is consumed by or provided by a plugin.</summary>
public enum ServiceKind
{
    /// <summary>The plugin depends on the service.</summary>
    Consumed,
    /// <summary>The plugin registers an implementation of the service.</summary>
    Provided
}
