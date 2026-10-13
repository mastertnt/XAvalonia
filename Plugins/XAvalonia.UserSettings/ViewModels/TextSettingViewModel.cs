using ReactiveUI;
using XAvalonia.Shell.Abstractions.Shell;

namespace XAvalonia.UserSettings.ViewModels;

/// <summary>Settings editor row for a <see cref="string"/> setting, edited with a text box.</summary>
public sealed class TextSettingViewModel : SettingViewModel
{
    private string? mValue;

    /// <summary>Initializes the row and reads the current value.</summary>
    public TextSettingViewModel(UserSettingDescriptor pSetting, IUserConfiguration pConfiguration, Action<string?> pReportError)
        : base(pSetting, pConfiguration, pReportError)
    {
        Refresh();
    }

    /// <summary>Current value.</summary>
    public string? Value
    {
        get => mValue;
        set
        {
            if (Equals(mValue, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref mValue, value);
            Commit(value);
        }
    }

    /// <inheritdoc/>
    protected override void OnValueRefreshed(object? pValue)
    {
        Value = pValue as string;
    }
}
