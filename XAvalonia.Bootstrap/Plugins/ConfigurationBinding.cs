using System.Globalization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using XAvalonia.Shell.Abstractions.Plugins;

namespace XAvalonia.Bootstrap.Plugins;

/// <summary>
/// Shared helpers that map plugin properties decorated with <see cref="ConfigurationKeyAttribute"/>
/// to and from JSON configuration values.
/// </summary>
internal static class ConfigurationBinding
{
    /// <summary>
    /// Returns the configuration section key of a plugin type:
    /// <see cref="ConfigurationSectionAttribute"/> when present, otherwise the class name without the <c>Plugin</c> suffix.
    /// </summary>
    public static string GetSectionKey(Type pPluginType)
    {
        ConfigurationSectionAttribute? lSectionAttr = pPluginType.GetCustomAttribute<ConfigurationSectionAttribute>();
        if (lSectionAttr is not null)
        {
            return lSectionAttr.SectionName;
        }

        return pPluginType.Name.EndsWith("Plugin", StringComparison.Ordinal) ? pPluginType.Name[..^6] : pPluginType.Name;
    }

    /// <summary>Returns <c>true</c> when values of <paramref name="pType"/> can be bound from JSON.</summary>
    public static bool IsSupportedType(Type pType)
    {
        Type lType = Nullable.GetUnderlyingType(pType) ?? pType;
        return lType == typeof(string) || lType == typeof(int) || lType == typeof(double)
            || lType == typeof(bool) || lType.IsEnum;
    }

    /// <summary>
    /// Converts a JSON value to <paramref name="pType"/>.
    /// Returns <c>null</c> for a JSON null or when the value does not match the type.
    /// </summary>
    public static object? ConvertJsonValue(Type pType, JsonElement pValue)
    {
        if (pValue.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        Type lType = Nullable.GetUnderlyingType(pType) ?? pType;
        try
        {
            if (lType == typeof(string)) return pValue.GetString();
            if (lType == typeof(int))    return pValue.GetInt32();
            if (lType == typeof(double)) return pValue.GetDouble();
            if (lType == typeof(bool))   return pValue.GetBoolean();
            if (lType.IsEnum && pValue.ValueKind == JsonValueKind.String
                && Enum.TryParse(lType, pValue.GetString(), true, out object? lEnumValue))
            {
                return lEnumValue;
            }
        }
        catch (Exception lEx) when (lEx is InvalidOperationException or FormatException)
        {
            // Value of the wrong JSON kind: ignored, the property keeps its current value.
        }

        return null;
    }

    /// <summary>
    /// Converts an arbitrary CLR value (e.g. coming from an editor control) to <paramref name="pType"/>.
    /// </summary>
    /// <exception cref="ArgumentException">The value cannot be converted.</exception>
    public static object? ConvertValue(Type pType, object? pValue)
    {
        Type? lUnderlying = Nullable.GetUnderlyingType(pType);
        if (pValue is null)
        {
            if (lUnderlying is not null || !pType.IsValueType)
            {
                return null;
            }

            throw new ArgumentException($"A value of type '{pType.Name}' is required.", nameof(pValue));
        }

        Type lType = lUnderlying ?? pType;
        if (lType.IsInstanceOfType(pValue))
        {
            return pValue;
        }

        try
        {
            if (lType.IsEnum)
            {
                return pValue is string lName
                    ? Enum.Parse(lType, lName, true)
                    : Enum.ToObject(lType, pValue);
            }

            return Convert.ChangeType(pValue, lType, CultureInfo.InvariantCulture);
        }
        catch (Exception lEx) when (lEx is InvalidCastException or FormatException or OverflowException or ArgumentException)
        {
            throw new ArgumentException($"'{pValue}' cannot be converted to '{lType.Name}'.", nameof(pValue), lEx);
        }
    }

    /// <summary>Converts a bound value to its JSON representation (enums are written by name).</summary>
    public static JsonNode? ToJsonNode(object? pValue)
    {
        return pValue switch
        {
            null          => null,
            string lText  => JsonValue.Create(lText),
            bool lBool    => JsonValue.Create(lBool),
            int lInt      => JsonValue.Create(lInt),
            double lDbl   => JsonValue.Create(lDbl),
            Enum lEnum    => JsonValue.Create(lEnum.ToString()),
            _             => JsonValue.Create(pValue.ToString()),
        };
    }

    /// <summary>Returns the public writable instance properties of a plugin decorated with <typeparamref name="TAttr"/>.</summary>
    public static IEnumerable<(PropertyInfo Property, TAttr Attribute)> GetBoundProperties<TAttr>(Type pPluginType)
        where TAttr : ConfigurationKeyAttribute
    {
        foreach (PropertyInfo lProp in pPluginType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            TAttr? lAttr = lProp.GetCustomAttribute<TAttr>();
            if (lAttr is not null && lProp.CanWrite && lProp.CanRead)
            {
                yield return (lProp, lAttr);
            }
        }
    }
}
