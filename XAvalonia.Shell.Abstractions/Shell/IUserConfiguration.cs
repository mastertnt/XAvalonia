namespace XAvalonia.Shell.Abstractions.Shell;

/// <summary>
/// Provides read and write access to plugin-specific sections in <c>user_configuration.json</c>.
/// Properties decorated with <c>[UserConfiguration("key")]</c> are populated from this file
/// and listed in <see cref="Settings"/>, so that a settings editor can display and change them.
/// </summary>
public interface IUserConfiguration : IJsonSectionProvider
{
    /// <summary>
    /// Raised after a setting value changed through <see cref="SetValue"/> or <see cref="ResetValue"/>.
    /// The plugin property bound to the setting already holds the new value.
    /// </summary>
    event EventHandler<UserSettingChangedEventArgs>? SettingChanged;

    /// <summary>Full path of the user configuration file.</summary>
    string FilePath { get; }

    /// <summary>All settings declared by the loaded plugins, ordered by category then by order.</summary>
    IReadOnlyList<UserSettingDescriptor> Settings { get; }

    /// <summary>Returns the current value of <paramref name="pSetting"/>.</summary>
    object? GetValue(UserSettingDescriptor pSetting);

    /// <summary>Returns <c>true</c> when <paramref name="pSetting"/> is explicitly set in the configuration file.</summary>
    bool IsModified(UserSettingDescriptor pSetting);

    /// <summary>
    /// Changes the value of <paramref name="pSetting"/>: updates the bound plugin property,
    /// raises <see cref="SettingChanged"/>, then writes the configuration file.
    /// Numeric values are clamped to <see cref="UserSettingDescriptor.Minimum"/> / <see cref="UserSettingDescriptor.Maximum"/>.
    /// </summary>
    /// <param name="pSetting">The setting to change.</param>
    /// <param name="pValue">New value; converted to <see cref="UserSettingDescriptor.ValueType"/>.</param>
    /// <exception cref="ArgumentException">The value cannot be converted to the setting type.</exception>
    /// <exception cref="IOException">
    /// The configuration file could not be written; the new value still applies until the application exits.
    /// </exception>
    void SetValue(UserSettingDescriptor pSetting, object? pValue);

    /// <summary>
    /// Removes <paramref name="pSetting"/> from the configuration file and restores its default value.
    /// </summary>
    /// <exception cref="IOException">The configuration file could not be written.</exception>
    void ResetValue(UserSettingDescriptor pSetting);
}
