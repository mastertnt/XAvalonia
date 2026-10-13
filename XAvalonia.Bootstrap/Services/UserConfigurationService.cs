using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using XAvalonia.Bootstrap.Plugins;
using XAvalonia.Shell.Abstractions.Plugins;
using XAvalonia.Shell.Abstractions.Shell;

namespace XAvalonia.Bootstrap.Services;

/// <summary>
/// Provides read and write access to plugin-specific sections in <c>user_configuration.json</c>.
/// Binds the <c>[UserConfiguration]</c> properties of each plugin, keeps track of their default values,
/// and writes changes back to the file.
/// Registered as singleton under both <c>UserConfigurationService</c> and <c>IUserConfiguration</c>.
/// </summary>
public sealed class UserConfigurationService : IUserConfiguration
{
    private static readonly JsonSerializerOptions WriteOptions = new JsonSerializerOptions { WriteIndented = true };

    private readonly JsonObject mRoot;
    private readonly bool mIsReadOnly;
    private readonly List<UserSettingDescriptor> mSettings = new List<UserSettingDescriptor>();
    private readonly Dictionary<UserSettingDescriptor, (object Plugin, PropertyInfo Property)> mBindings =
        new Dictionary<UserSettingDescriptor, (object Plugin, PropertyInfo Property)>();

    /// <summary>
    /// Initializes the service from <paramref name="pFilePath"/>.
    /// A missing file is treated as empty and created on the first change.
    /// A malformed file is ignored and never overwritten.
    /// </summary>
    /// <param name="pFilePath">Full path of <c>user_configuration.json</c>.</param>
    public UserConfigurationService(string pFilePath)
    {
        FilePath = pFilePath;
        mRoot = new JsonObject();

        if (!File.Exists(pFilePath))
        {
            return;
        }

        try
        {
            JsonNode? lNode = JsonNode.Parse(File.ReadAllText(pFilePath),
                documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
            if (lNode is JsonObject lObject)
            {
                mRoot = lObject;
                return;
            }
        }
        catch (JsonException lEx)
        {
            Trace.WriteLine($"[UserConfiguration] '{pFilePath}' is not valid JSON: {lEx.Message}");
        }

        // Keep the user's file untouched rather than replacing what we could not read.
        mIsReadOnly = true;
        Trace.WriteLine($"[UserConfiguration] '{pFilePath}' ignored; changes will not be saved.");
    }

    /// <inheritdoc/>
    public event EventHandler<UserSettingChangedEventArgs>? SettingChanged;

    /// <inheritdoc/>
    public string FilePath { get; }

    /// <inheritdoc/>
    public IReadOnlyList<UserSettingDescriptor> Settings => mSettings
        .OrderBy(pS => pS.Category, StringComparer.CurrentCultureIgnoreCase)
        .ThenBy(pS => pS.Order)
        .ThenBy(pS => pS.DisplayName, StringComparer.CurrentCultureIgnoreCase)
        .ToList();

    /// <inheritdoc/>
    public JsonElement? GetSection(string pKey)
    {
        if (mRoot.TryGetPropertyValue(pKey, out JsonNode? lNode))
        {
            return lNode is null
                ? JsonDocument.Parse("null").RootElement.Clone()
                : JsonSerializer.SerializeToElement(lNode);
        }

        return null;
    }

    /// <summary>
    /// Describes every <c>[UserConfiguration]</c> property of <paramref name="pPlugin"/>,
    /// records its current value as the default, then applies the value found in the file.
    /// Called by <see cref="PluginManager"/> before <see cref="IPlugin.Initialize"/>.
    /// </summary>
    public void RegisterPlugin(IPlugin pPlugin)
    {
        Type lType = pPlugin.GetType();
        string lSection = ConfigurationBinding.GetSectionKey(lType);
        JsonObject? lSectionNode = mRoot[lSection] as JsonObject;

        foreach ((PropertyInfo lProp, UserConfigurationAttribute lAttr) in ConfigurationBinding.GetBoundProperties<UserConfigurationAttribute>(lType))
        {
            if (!ConfigurationBinding.IsSupportedType(lProp.PropertyType))
            {
                Trace.WriteLine($"[UserConfiguration] '{lSection}.{lAttr.Key}': type '{lProp.PropertyType.Name}' is not supported.");
                continue;
            }

            UserSettingDescriptor lSetting = new UserSettingDescriptor(
                pPluginId: pPlugin.Id,
                pPluginName: pPlugin.Name,
                pSection: lSection,
                pKey: lAttr.Key,
                pValueType: lProp.PropertyType,
                pDefaultValue: lProp.GetValue(pPlugin),
                pDisplayName: lAttr.DisplayName ?? Humanize(lAttr.Key),
                pDescription: lAttr.Description ?? string.Empty,
                pCategory: string.IsNullOrWhiteSpace(lAttr.Category) ? pPlugin.Name : lAttr.Category,
                pOrder: lAttr.Order,
                pMinimum: double.IsNaN(lAttr.Minimum) ? null : lAttr.Minimum,
                pMaximum: double.IsNaN(lAttr.Maximum) ? null : lAttr.Maximum);

            mSettings.Add(lSetting);
            mBindings[lSetting] = (pPlugin, lProp);

            if (lSectionNode is not null && lSectionNode.TryGetPropertyValue(lAttr.Key, out JsonNode? lValueNode))
            {
                JsonElement lValue = lValueNode is null
                    ? JsonDocument.Parse("null").RootElement
                    : JsonSerializer.SerializeToElement(lValueNode);
                object? lConverted = ConfigurationBinding.ConvertJsonValue(lProp.PropertyType, lValue);
                if (lConverted is not null || lValue.ValueKind == JsonValueKind.Null && IsNullable(lProp.PropertyType))
                {
                    lProp.SetValue(pPlugin, lConverted);
                }
            }
        }
    }

    /// <summary>Forgets the settings of a plugin that failed to initialize.</summary>
    public void UnregisterPlugin(string pPluginId)
    {
        foreach (UserSettingDescriptor lSetting in mSettings.Where(pS => pS.PluginId == pPluginId).ToList())
        {
            mSettings.Remove(lSetting);
            mBindings.Remove(lSetting);
        }
    }

    /// <inheritdoc/>
    public object? GetValue(UserSettingDescriptor pSetting)
    {
        (object lPlugin, PropertyInfo lProp) = GetBinding(pSetting);
        return lProp.GetValue(lPlugin);
    }

    /// <inheritdoc/>
    public bool IsModified(UserSettingDescriptor pSetting)
    {
        return mRoot[pSetting.Section] is JsonObject lSection && lSection.ContainsKey(pSetting.Key);
    }

    /// <inheritdoc/>
    public void SetValue(UserSettingDescriptor pSetting, object? pValue)
    {
        (object lPlugin, PropertyInfo lProp) = GetBinding(pSetting);
        object? lNewValue = Clamp(pSetting, ConfigurationBinding.ConvertValue(pSetting.ValueType, pValue));
        object? lOldValue = lProp.GetValue(lPlugin);

        if (mRoot[pSetting.Section] is not JsonObject lSection)
        {
            lSection = new JsonObject();
            mRoot[pSetting.Section] = lSection;
        }

        lSection[pSetting.Key] = ConfigurationBinding.ToJsonNode(lNewValue);
        lProp.SetValue(lPlugin, lNewValue);
        if (!Equals(lOldValue, lNewValue))
        {
            SettingChanged?.Invoke(this, new UserSettingChangedEventArgs(pSetting, lOldValue, lNewValue));
        }

        Save();
    }

    /// <inheritdoc/>
    public void ResetValue(UserSettingDescriptor pSetting)
    {
        (object lPlugin, PropertyInfo lProp) = GetBinding(pSetting);
        object? lOldValue = lProp.GetValue(lPlugin);

        if (mRoot[pSetting.Section] is JsonObject lSection)
        {
            lSection.Remove(pSetting.Key);
            if (lSection.Count == 0)
            {
                mRoot.Remove(pSetting.Section);
            }
        }

        lProp.SetValue(lPlugin, pSetting.DefaultValue);
        if (!Equals(lOldValue, pSetting.DefaultValue))
        {
            SettingChanged?.Invoke(this, new UserSettingChangedEventArgs(pSetting, lOldValue, pSetting.DefaultValue));
        }

        Save();
    }

    // Writes the whole document back to FilePath. The new value is already applied in memory,
    // so a failure here only means it will not survive a restart.
    private void Save()
    {
        if (mIsReadOnly)
        {
            throw new IOException($"'{FilePath}' is not valid JSON and was not overwritten. Fix or delete it to save settings.");
        }

        string? lDirectory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(lDirectory))
        {
            Directory.CreateDirectory(lDirectory);
        }

        try
        {
            File.WriteAllText(FilePath, mRoot.ToJsonString(WriteOptions) + Environment.NewLine);
        }
        catch (UnauthorizedAccessException lEx)
        {
            throw new IOException($"'{FilePath}' cannot be written: {lEx.Message}", lEx);
        }
    }

    private (object Plugin, PropertyInfo Property) GetBinding(UserSettingDescriptor pSetting)
    {
        return mBindings.TryGetValue(pSetting, out (object Plugin, PropertyInfo Property) lBinding)
            ? lBinding
            : throw new ArgumentException($"Setting '{pSetting.FullKey}' is not registered.", nameof(pSetting));
    }

    private static object? Clamp(UserSettingDescriptor pSetting, object? pValue)
    {
        return pValue switch
        {
            int lInt       => (int)Math.Clamp(lInt, pSetting.Minimum ?? int.MinValue, pSetting.Maximum ?? int.MaxValue),
            double lDouble => Math.Clamp(lDouble, pSetting.Minimum ?? double.MinValue, pSetting.Maximum ?? double.MaxValue),
            _              => pValue,
        };
    }

    private static bool IsNullable(Type pType) => !pType.IsValueType || Nullable.GetUnderlyingType(pType) is not null;

    // "fontSize" → "Font Size", "editor.word_wrap" → "Editor Word Wrap".
    internal static string Humanize(string pKey)
    {
        StringBuilder lBuilder = new StringBuilder(pKey.Length + 8);
        bool lNewWord = true;
        char lPrevious = '\0';

        foreach (char lChar in pKey)
        {
            if (lChar is '.' or '_' or '-' or ' ')
            {
                lNewWord = true;
                lPrevious = lChar;
                continue;
            }

            bool lCamelBoundary = char.IsUpper(lChar) && char.IsLower(lPrevious);
            if ((lNewWord || lCamelBoundary) && lBuilder.Length > 0)
            {
                lBuilder.Append(' ');
            }

            lBuilder.Append(lNewWord || lCamelBoundary ? char.ToUpperInvariant(lChar) : lChar);
            lNewWord = false;
            lPrevious = lChar;
        }

        return lBuilder.ToString();
    }
}
