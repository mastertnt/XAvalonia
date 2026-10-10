namespace XAvalonia.Shell.Abstractions.Plugins;

/// <summary>
/// Declares that a plugin must be registered and initialized after another plugin,
/// identified by its <see cref="IPlugin.Id"/>.
/// Can be applied several times to declare several dependencies.
/// </summary>
/// <example>
/// <code>
/// [DependsOnPlugin("org.avaloniaui.shell.log-viewer")]
/// [DependsOnPlugin("com.mycompany.optional", Optional = true)]
/// public sealed class MyPlugin : IPlugin { … }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class DependsOnPluginAttribute : Attribute
{
    /// <param name="pPluginId">Identifier of the plugin this plugin depends on.</param>
    public DependsOnPluginAttribute(string pPluginId)
    {
        PluginId = pPluginId;
    }

    /// <summary>Identifier of the plugin this plugin depends on.</summary>
    public string PluginId { get; }

    /// <summary>
    /// When <c>true</c>, a missing dependency is ignored instead of aborting startup.
    /// The ordering constraint still applies when the dependency is present.
    /// </summary>
    public bool Optional { get; set; }
}
