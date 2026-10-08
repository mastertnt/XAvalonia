namespace XAvalonia.Shell.Abstractions.Plugins;

/// <summary>
/// Marks a plugin property to be populated from the plugin's section in
/// <c>user_configuration.json</c> before <see cref="IPlugin.Initialize"/> is called.
/// The section key is the plugin class name without the <c>Plugin</c> suffix
/// (e.g. <c>LogViewerPlugin</c> → <c>"LogViewer"</c>),
/// unless overridden with <see cref="ConfigurationSectionAttribute"/>.
/// </summary>
/// <example>
/// <code>
/// [UserConfiguration("theme")]
/// public string Theme { get; set; } = "Light";
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class UserConfigurationAttribute : ConfigurationKeyAttribute
{
    /// <param name="pKey">Key inside the plugin's JSON section.</param>
    public UserConfigurationAttribute(string pKey) : base(pKey) { }
}
