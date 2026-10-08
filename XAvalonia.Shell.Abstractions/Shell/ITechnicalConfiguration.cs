namespace XAvalonia.Shell.Abstractions.Shell;

/// <summary>
/// Provides read access to plugin-specific sections in <c>technical_configuration.json</c>.
/// Properties decorated with <c>[TechConfiguration("key")]</c> are populated from this file.
/// </summary>
public interface ITechnicalConfiguration : IJsonSectionProvider { }
