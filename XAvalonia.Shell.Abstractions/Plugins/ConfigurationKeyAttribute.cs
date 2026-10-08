namespace XAvalonia.Shell.Abstractions.Plugins;

/// <summary>
/// Abstract base for configuration-binding attributes.
/// Derived types target a specific configuration file
/// (<c>technical_configuration.json</c> or <c>user_configuration.json</c>).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class ConfigurationKeyAttribute : Attribute
{
    /// <param name="pKey">Key inside the plugin's JSON section.</param>
    protected ConfigurationKeyAttribute(string pKey)
    {
        Key = pKey;
    }

    /// <summary>Key inside the plugin's JSON section.</summary>
    public string Key { get; }
}
