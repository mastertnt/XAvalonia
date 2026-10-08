namespace XAvalonia.Shell.Abstractions.Plugins;

/// <summary>
/// Overrides the configuration section name used for a plugin class.
/// When absent, the section name is derived by stripping the
/// <c>Plugin</c> suffix from the class name (e.g. <c>LogViewerPlugin</c> → <c>"LogViewer"</c>).
/// </summary>
/// <example>
/// <code>
/// [ConfigurationSection("Avalonia.MainWindow")]
/// public sealed class AvaloniaMainWindowPlugin : IPlugin { … }
/// </code>
/// maps to:
/// <code>
/// { "Avalonia.MainWindow": { "name": "SampleApp" } }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ConfigurationSectionAttribute : Attribute
{
    /// <param name="pSectionName">Top-level key in the configuration file for this plugin.</param>
    public ConfigurationSectionAttribute(string pSectionName)
    {
        SectionName = pSectionName;
    }

    /// <summary>Top-level key in the configuration file for this plugin.</summary>
    public string SectionName { get; }
}
