namespace XAvalonia.Shell.Abstractions.Plugins;

/// <summary>
/// Marks a plugin property to be populated from the plugin's section in
/// <c>technical_configuration.json</c> before <see cref="IPlugin.Initialize"/> is called.
/// The section key is the plugin class name without the <c>Plugin</c> suffix
/// (e.g. <c>AvaloniaMainWindowPlugin</c> → <c>"AvaloniaMainWindow"</c>),
/// unless overridden with <see cref="ConfigurationSectionAttribute"/>.
/// </summary>
/// <example>
/// <code>
/// [TechConfiguration("logLevel")]
/// public string LogLevel { get; set; } = "Warning";
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class TechConfigurationAttribute : ConfigurationKeyAttribute
{
    /// <param name="pKey">Key inside the plugin's JSON section.</param>
    public TechConfigurationAttribute(string pKey) : base(pKey) { }
}
