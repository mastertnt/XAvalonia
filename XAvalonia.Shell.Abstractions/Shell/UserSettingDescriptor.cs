namespace XAvalonia.Shell.Abstractions.Shell;

/// <summary>
/// Describes one user setting declared by a plugin through <c>[UserConfiguration]</c>.
/// Instances are created by the shell and exposed through <see cref="IUserConfiguration.Settings"/>.
/// </summary>
public sealed class UserSettingDescriptor
{
    /// <summary>Initializes a setting descriptor.</summary>
    /// <param name="pPluginId">Identifier of the plugin that declares the setting.</param>
    /// <param name="pPluginName">Human-readable name of the plugin that declares the setting.</param>
    /// <param name="pSection">Top-level key of the plugin's section in <c>user_configuration.json</c>.</param>
    /// <param name="pKey">Key inside the plugin's section.</param>
    /// <param name="pValueType">CLR type of the bound property.</param>
    /// <param name="pDefaultValue">Value of the property before the configuration was applied.</param>
    /// <param name="pDisplayName">Label shown in a settings editor.</param>
    /// <param name="pDescription">Help text shown in a settings editor.</param>
    /// <param name="pCategory">Category path, segments separated by <c>/</c>.</param>
    /// <param name="pOrder">Relative position inside its category.</param>
    /// <param name="pMinimum">Smallest accepted numeric value, or <c>null</c>.</param>
    /// <param name="pMaximum">Largest accepted numeric value, or <c>null</c>.</param>
    public UserSettingDescriptor(
        string pPluginId,
        string pPluginName,
        string pSection,
        string pKey,
        Type pValueType,
        object? pDefaultValue,
        string pDisplayName,
        string pDescription,
        string pCategory,
        int pOrder = 100,
        double? pMinimum = null,
        double? pMaximum = null)
    {
        PluginId     = pPluginId;
        PluginName   = pPluginName;
        Section      = pSection;
        Key          = pKey;
        ValueType    = pValueType;
        DefaultValue = pDefaultValue;
        DisplayName  = pDisplayName;
        Description  = pDescription;
        Category     = pCategory;
        Order        = pOrder;
        Minimum      = pMinimum;
        Maximum      = pMaximum;
    }

    /// <summary>Identifier of the plugin that declares the setting.</summary>
    public string PluginId { get; }

    /// <summary>Human-readable name of the plugin that declares the setting.</summary>
    public string PluginName { get; }

    /// <summary>Top-level key of the plugin's section in <c>user_configuration.json</c>.</summary>
    public string Section { get; }

    /// <summary>Key inside the plugin's section.</summary>
    public string Key { get; }

    /// <summary>Fully qualified key, <c>Section.Key</c>, as shown in a settings editor.</summary>
    public string FullKey => $"{Section}.{Key}";

    /// <summary>CLR type of the bound property (string, bool, int, double, their nullable forms, or an enum).</summary>
    public Type ValueType { get; }

    /// <summary>Value of the property before the configuration was applied.</summary>
    public object? DefaultValue { get; }

    /// <summary>Label shown in a settings editor.</summary>
    public string DisplayName { get; }

    /// <summary>Help text shown in a settings editor.</summary>
    public string Description { get; }

    /// <summary>Category path, segments separated by <c>/</c>.</summary>
    public string Category { get; }

    /// <summary>Relative position inside its category.</summary>
    public int Order { get; }

    /// <summary>Smallest accepted numeric value, or <c>null</c> when unbounded.</summary>
    public double? Minimum { get; }

    /// <summary>Largest accepted numeric value, or <c>null</c> when unbounded.</summary>
    public double? Maximum { get; }
}
