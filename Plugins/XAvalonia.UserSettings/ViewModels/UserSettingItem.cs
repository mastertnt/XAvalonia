using ReactiveUI;
using XAvalonia.Shell.Abstractions.Properties;
using XAvalonia.Shell.Abstractions.Shell;

namespace XAvalonia.UserSettings.ViewModels;

/// <summary>
/// Adapts a <see cref="UserSettingDescriptor"/> to <see cref="IPropertyItem"/> so that a
/// <c>PropertyInspector</c> can display and edit it; reads and writes go through <see cref="IUserConfiguration"/>.
/// </summary>
public sealed class UserSettingItem : ReactiveObject, IPropertyItem
{
    private readonly IUserConfiguration mConfiguration;
    private readonly Action<string?> mReportError;
    private object? mValue;
    private bool mIsModified;

    /// <summary>Initializes the item and reads the current value.</summary>
    /// <param name="pSetting">The setting to expose.</param>
    /// <param name="pConfiguration">Configuration used to read and write the value.</param>
    /// <param name="pReportError">Receives the last save error, or <c>null</c> after a successful change.</param>
    public UserSettingItem(UserSettingDescriptor pSetting, IUserConfiguration pConfiguration, Action<string?> pReportError)
    {
        Setting        = pSetting;
        mConfiguration = pConfiguration;
        mReportError   = pReportError;
        Refresh();
    }

    /// <summary>The wrapped setting.</summary>
    public UserSettingDescriptor Setting { get; }

    /// <inheritdoc/>
    public string Key => Setting.FullKey;

    /// <inheritdoc/>
    public string DisplayName => Setting.DisplayName;

    /// <inheritdoc/>
    public string Description => Setting.Description;

    /// <inheritdoc/>
    public string Category => Setting.Category;

    /// <inheritdoc/>
    public Type ValueType => Setting.ValueType;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public double? Minimum => Setting.Minimum;

    /// <inheritdoc/>
    public double? Maximum => Setting.Maximum;

    /// <inheritdoc/>
    public object? Value
    {
        get => mValue;
        set
        {
            if (Equals(mValue, value))
            {
                return;
            }

            try
            {
                mConfiguration.SetValue(Setting, value);
                mReportError(null);
            }
            catch (Exception lEx) when (lEx is ArgumentException or IOException)
            {
                mReportError(lEx.Message);
            }

            Refresh();
        }
    }

    /// <inheritdoc/>
    public bool IsModified
    {
        get => mIsModified;
        private set => this.RaiseAndSetIfChanged(ref mIsModified, value);
    }

    /// <inheritdoc/>
    public void Reset()
    {
        try
        {
            mConfiguration.ResetValue(Setting);
            mReportError(null);
        }
        catch (IOException lEx)
        {
            mReportError(lEx.Message);
        }

        Refresh();
    }

    /// <summary>Re-reads the value and the modified state from the configuration.</summary>
    public void Refresh()
    {
        // Always notified, even when unchanged (e.g. a clamped value), so editors show what was kept.
        mValue = mConfiguration.GetValue(Setting);
        this.RaisePropertyChanged(nameof(Value));
        IsModified = mConfiguration.IsModified(Setting);
    }
}
