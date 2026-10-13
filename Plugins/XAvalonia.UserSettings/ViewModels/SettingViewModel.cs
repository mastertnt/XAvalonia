using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using XAvalonia.Shell.Abstractions.Shell;

namespace XAvalonia.UserSettings.ViewModels;

/// <summary>
/// Base view model for one row of the settings editor.
/// Derived types expose a typed <c>Value</c> bound to the matching editor control.
/// </summary>
public abstract class SettingViewModel : ReactiveObject
{
    private readonly IUserConfiguration mConfiguration;
    private readonly Action<string?> mReportError;
    private bool mIsModified;
    private bool mIsRefreshing;

    /// <summary>Initializes the row for <paramref name="pSetting"/>.</summary>
    /// <param name="pSetting">The setting displayed by this row.</param>
    /// <param name="pConfiguration">Configuration used to read and write the value.</param>
    /// <param name="pReportError">Callback receiving the last error message, or <c>null</c> on success.</param>
    protected SettingViewModel(UserSettingDescriptor pSetting, IUserConfiguration pConfiguration, Action<string?> pReportError)
    {
        Setting        = pSetting;
        mConfiguration = pConfiguration;
        mReportError   = pReportError;
        ResetCommand   = new RelayCommand(Reset);
    }

    /// <summary>The setting displayed by this row.</summary>
    public UserSettingDescriptor Setting { get; }

    /// <summary>Category path shown before the title (e.g. <c>Text Editor › Font</c>).</summary>
    public string Breadcrumb => Setting.Category.Replace("/", " › ", StringComparison.Ordinal);

    /// <summary>Setting label.</summary>
    public string Title => Setting.DisplayName;

    /// <summary>Help text.</summary>
    public string Description => Setting.Description;

    /// <summary>Whether the help text is shown above the editor (check boxes show it as their label instead).</summary>
    public virtual bool ShowDescription => Description.Length > 0;

    /// <summary>Fully qualified JSON key, shown as a tooltip.</summary>
    public string FullKey => Setting.FullKey;

    /// <summary>Whether the setting is explicitly set in the user configuration file.</summary>
    public bool IsModified
    {
        get => mIsModified;
        private set => this.RaiseAndSetIfChanged(ref mIsModified, value);
    }

    /// <summary>Restores the default value.</summary>
    public ICommand ResetCommand { get; }

    /// <summary>Re-reads the current value from the configuration.</summary>
    public void Refresh()
    {
        mIsRefreshing = true;
        try
        {
            OnValueRefreshed(mConfiguration.GetValue(Setting));
            IsModified = mConfiguration.IsModified(Setting);
        }
        finally
        {
            mIsRefreshing = false;
        }
    }

    /// <summary>Returns <c>true</c> when every term occurs in the title, key, category or description.</summary>
    public bool Matches(IReadOnlyList<string> pTerms)
    {
        foreach (string lTerm in pTerms)
        {
            bool lFound = Title.Contains(lTerm, StringComparison.CurrentCultureIgnoreCase)
                || FullKey.Contains(lTerm, StringComparison.OrdinalIgnoreCase)
                || Setting.Category.Contains(lTerm, StringComparison.CurrentCultureIgnoreCase)
                || Description.Contains(lTerm, StringComparison.CurrentCultureIgnoreCase);
            if (!lFound)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>Pushes the current value to the editor property without writing it back.</summary>
    protected abstract void OnValueRefreshed(object? pValue);

    /// <summary>Writes <paramref name="pValue"/> to the configuration, then re-reads the stored value.</summary>
    protected void Commit(object? pValue)
    {
        if (mIsRefreshing)
        {
            return;
        }

        try
        {
            mConfiguration.SetValue(Setting, pValue);
            mReportError(null);
        }
        catch (Exception lEx) when (lEx is ArgumentException or IOException)
        {
            mReportError(lEx.Message);
        }

        Refresh();
    }

    private void Reset()
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
}
