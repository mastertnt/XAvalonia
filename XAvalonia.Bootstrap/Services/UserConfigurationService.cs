using System.Text.Json;
using XAvalonia.Shell.Abstractions.Shell;

namespace XAvalonia.Bootstrap.Services;

/// <summary>
/// Provides read access to plugin-specific sections in <c>user_configuration.json</c>.
/// Registered as singleton under both <c>UserConfigurationService</c> and <c>IUserConfiguration</c>.
/// </summary>
public sealed class UserConfigurationService : IUserConfiguration, IDisposable
{
    private readonly JsonDocument mDocument;

    /// <summary>Initializes the service from the parsed <c>user_configuration.json</c> document.</summary>
    public UserConfigurationService(JsonDocument pDocument)
    {
        mDocument = pDocument;
    }

    /// <inheritdoc/>
    public JsonElement? GetSection(string pKey)
    {
        if (mDocument.RootElement.TryGetProperty(pKey, out JsonElement lElement))
        {
            return lElement;
        }

        return null;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        mDocument.Dispose();
    }
}
