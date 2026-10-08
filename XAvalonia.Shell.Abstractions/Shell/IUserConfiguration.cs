namespace XAvalonia.Shell.Abstractions.Shell;

/// <summary>
/// Provides read access to plugin-specific sections in <c>user_configuration.json</c>.
/// Properties decorated with <c>[UserConfiguration("key")]</c> are populated from this file.
/// </summary>
public interface IUserConfiguration : IJsonSectionProvider { }
