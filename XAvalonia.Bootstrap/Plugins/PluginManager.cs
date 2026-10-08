using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Shell.Abstractions.Plugins;
using XAvalonia.Shell.Abstractions.Shell;
#pragma warning disable CS8714 // generic constraint nullable

namespace XAvalonia.Bootstrap.Plugins;

/// <summary>
/// Manages the plugin lifecycle: Phase 1 service registration and Phase 2 initialization.
/// Registered in DI under both <see cref="PluginManager"/> and <see cref="IPluginManager"/>.
/// </summary>
public sealed class PluginManager : IPluginManager
{
    private static readonly HttpClient SharedHttpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private readonly IReadOnlyList<IPlugin> mPlugins;
    private readonly Dictionary<string, Version?> mVersionCache = new Dictionary<string, Version?>();
    private readonly Dictionary<string, IReadOnlyList<Type>> mConsumedServicesByPluginId =
        new Dictionary<string, IReadOnlyList<Type>>();

    /// <summary>Initializes the manager with the plugins discovered at startup.</summary>
    /// <param name="pPlugins">Plugins discovered by <see cref="PluginLoader"/>.</param>
    public PluginManager(IReadOnlyList<IPlugin> pPlugins)
    {
        mPlugins = pPlugins;
    }

    /// <inheritdoc/>
    public event EventHandler? AllPluginsLoaded;

    /// <inheritdoc/>
    public IReadOnlyList<IPlugin> Plugins => mPlugins;

    /// <inheritdoc/>
    public IReadOnlyList<Type> GetConsumedServices(string pPluginId)
    {
        return mConsumedServicesByPluginId.TryGetValue(pPluginId, out IReadOnlyList<Type>? lServices)
            ? lServices
            : Array.Empty<Type>();
    }

    /// <summary>Phase 1 — calls <see cref="IPlugin.RegisterServices"/> on every plugin.</summary>
    internal void RegisterAllServices(IServiceCollection pServices)
    {
        foreach (IPlugin lPlugin in mPlugins)
        {
            try
            {
                Trace.WriteLine($"[PluginManager] '{lPlugin.Id}' loaded");
                lPlugin.RegisterServices(pServices);
            }
            catch (Exception lEx)
            {
                Trace.WriteLine($"[PluginManager] '{lPlugin.Id}' RegisterServices failed: {lEx.Message}");
                throw;
            }
        }
    }

    /// <summary>Phase 2 — calls <see cref="IPlugin.Initialize"/> on every plugin, then raises <see cref="AllPluginsLoaded"/>.</summary>
    internal void InitializeAll(IServiceProvider pServiceProvider)
    {
        ITechnicalConfiguration?  lTechConfig  = pServiceProvider.GetService<ITechnicalConfiguration>();
        IUserConfiguration?       lUserConfig  = pServiceProvider.GetService<IUserConfiguration>();

        foreach (IPlugin lPlugin in mPlugins)
        {
            try
            {
                if (lTechConfig  is not null) BindConfiguration<TechConfigurationAttribute>(lPlugin, lTechConfig);
                if (lUserConfig  is not null) BindConfiguration<UserConfigurationAttribute>(lPlugin, lUserConfig);

                PluginServiceManager lServiceManager = new PluginServiceManager(pServiceProvider);
                lPlugin.Initialize(lServiceManager);
                mConsumedServicesByPluginId[lPlugin.Id] = lServiceManager.ConsumedServices;
            }
            catch (Exception lEx)
            {
                Trace.WriteLine($"[PluginManager] '{lPlugin.Id}' Initialize failed: {lEx.Message}");
                throw;
            }
        }

        AllPluginsLoaded?.Invoke(this, EventArgs.Empty);
    }

    // Populates properties decorated with TAttr from the plugin's section in the given config provider.
    // Section key = [ConfigurationSection] when present, otherwise class name without "Plugin" suffix.
    private static void BindConfiguration<TAttr>(IPlugin pPlugin, IJsonSectionProvider pConfig)
        where TAttr : ConfigurationKeyAttribute
    {
        Type lType = pPlugin.GetType();

        ConfigurationSectionAttribute? lSectionAttr = lType.GetCustomAttribute<ConfigurationSectionAttribute>();
        string lSectionKey = lSectionAttr is not null
            ? lSectionAttr.SectionName
            : (lType.Name.EndsWith("Plugin", StringComparison.Ordinal) ? lType.Name[..^6] : lType.Name);

        JsonElement? lSection = pConfig.GetSection(lSectionKey);
        if (lSection is null)
        {
            return;
        }

        foreach (PropertyInfo lProp in lType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            TAttr? lAttr = lProp.GetCustomAttribute<TAttr>();
            if (lAttr is null || !lProp.CanWrite)
            {
                continue;
            }

            if (!lSection.Value.TryGetProperty(lAttr.Key, out JsonElement lValue))
            {
                continue;
            }

            object? lConverted = ConvertJsonValue(lProp.PropertyType, lValue);
            if (lConverted is not null)
            {
                lProp.SetValue(pPlugin, lConverted);
            }
        }
    }

    private static object? ConvertJsonValue(Type pType, JsonElement pValue)
    {
        if (pType == typeof(string))   return pValue.GetString();
        if (pType == typeof(int))      return pValue.GetInt32();
        if (pType == typeof(double))   return pValue.GetDouble();
        if (pType == typeof(bool))     return pValue.GetBoolean();
        if (pType == typeof(int?))     return pValue.ValueKind == JsonValueKind.Null ? null : (object)pValue.GetInt32();
        if (pType == typeof(double?))  return pValue.ValueKind == JsonValueKind.Null ? null : (object)pValue.GetDouble();
        if (pType == typeof(bool?))    return pValue.ValueKind == JsonValueKind.Null ? null : (object)pValue.GetBoolean();
        return null;
    }

    /// <inheritdoc/>
    public async Task<Version?> GetAvailableVersionAsync(string pNuGetPackageId, CancellationToken pCancellationToken = default)
    {
        string lKey = pNuGetPackageId.ToLowerInvariant();

        if (mVersionCache.TryGetValue(lKey, out Version? lCached))
        {
            return lCached;
        }

        Version? lVersion = await FetchLatestStableVersionAsync(lKey, pCancellationToken).ConfigureAwait(false);
        mVersionCache[lKey] = lVersion;
        return lVersion;
    }

    private static async Task<Version?> FetchLatestStableVersionAsync(string pPackageId, CancellationToken pCancellationToken)
    {
        string lUrl = $"https://api.nuget.org/v3-flatcontainer/{pPackageId}/index.json";

        try
        {
            string lJson = await SharedHttpClient.GetStringAsync(lUrl, pCancellationToken).ConfigureAwait(false);
            NuGetVersionIndex? lIndex = JsonSerializer.Deserialize<NuGetVersionIndex>(lJson);

            if (lIndex is null || lIndex.Versions.Count == 0)
            {
                return null;
            }

            string? lLatestStable = lIndex.Versions
                .Where(pV => !pV.Contains('-'))
                .LastOrDefault();

            return lLatestStable is null ? null : Version.Parse(lLatestStable);
        }
        catch (Exception lEx)
        {
            Trace.WriteLine($"[PluginManager] NuGet query failed for '{pPackageId}': {lEx.Message}");
            return null;
        }
    }

    private sealed class NuGetVersionIndex
    {
        [JsonPropertyName("versions")]
        public IReadOnlyList<string> Versions { get; set; } = Array.Empty<string>();
    }
}
