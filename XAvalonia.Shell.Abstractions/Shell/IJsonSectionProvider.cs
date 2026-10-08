using System.Text.Json;

namespace XAvalonia.Shell.Abstractions.Shell;

/// <summary>
/// Provides read access to named JSON sections from a configuration file.
/// Base contract shared by <c>ITechnicalConfiguration</c> and <c>IUserConfiguration</c>.
/// </summary>
public interface IJsonSectionProvider
{
    /// <summary>
    /// Returns the JSON section associated with <paramref name="pKey"/>,
    /// or <c>null</c> if the key is absent.
    /// </summary>
    /// <param name="pKey">Top-level key in the configuration file.</param>
    JsonElement? GetSection(string pKey);
}
