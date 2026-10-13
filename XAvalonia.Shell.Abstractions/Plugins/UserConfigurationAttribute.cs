namespace XAvalonia.Shell.Abstractions.Plugins;

/// <summary>
/// Marks a plugin property to be populated from the plugin's section in
/// <c>user_configuration.json</c> before <see cref="IPlugin.Initialize"/> is called.
/// The section key is the plugin class name without the <c>Plugin</c> suffix
/// (e.g. <c>LogViewerPlugin</c> → <c>"LogViewer"</c>),
/// unless overridden with <see cref="ConfigurationSectionAttribute"/>.
/// The optional named properties describe how the setting is presented in a settings editor.
/// </summary>
/// <example>
/// <code>
/// [UserConfiguration("theme", DisplayName = "Theme", Category = "Appearance", Description = "Color theme of the shell.")]
/// public string Theme { get; set; } = "Light";
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class UserConfigurationAttribute : ConfigurationKeyAttribute
{
    /// <param name="pKey">Key inside the plugin's JSON section.</param>
    public UserConfigurationAttribute(string pKey) : base(pKey) { }

    /// <summary>Label shown in the settings editor. Defaults to the key, humanized (e.g. <c>fontSize</c> → <c>Font Size</c>).</summary>
    public string? DisplayName { get; set; }

    /// <summary>Help text shown under the label in the settings editor.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category path in the settings editor tree; segments are separated by <c>/</c>
    /// (e.g. <c>"Text Editor/Font"</c>). Defaults to the plugin's <see cref="IPlugin.Name"/>.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>Relative position inside its category; settings are sorted in ascending order.</summary>
    public int Order { get; set; } = 100;

    /// <summary>Smallest accepted value for numeric settings. <see cref="double.NaN"/> means unbounded.</summary>
    public double Minimum { get; set; } = double.NaN;

    /// <summary>Largest accepted value for numeric settings. <see cref="double.NaN"/> means unbounded.</summary>
    public double Maximum { get; set; } = double.NaN;
}
