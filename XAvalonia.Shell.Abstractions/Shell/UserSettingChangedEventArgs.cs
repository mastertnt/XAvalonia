namespace XAvalonia.Shell.Abstractions.Shell;

/// <summary>Data for <see cref="IUserConfiguration.SettingChanged"/>.</summary>
public sealed class UserSettingChangedEventArgs : EventArgs
{
    /// <summary>Initializes the event data.</summary>
    /// <param name="pSetting">The setting whose value changed.</param>
    /// <param name="pOldValue">Value before the change.</param>
    /// <param name="pNewValue">Value after the change.</param>
    public UserSettingChangedEventArgs(UserSettingDescriptor pSetting, object? pOldValue, object? pNewValue)
    {
        Setting  = pSetting;
        OldValue = pOldValue;
        NewValue = pNewValue;
    }

    /// <summary>The setting whose value changed.</summary>
    public UserSettingDescriptor Setting { get; }

    /// <summary>Value before the change.</summary>
    public object? OldValue { get; }

    /// <summary>Value after the change.</summary>
    public object? NewValue { get; }
}
