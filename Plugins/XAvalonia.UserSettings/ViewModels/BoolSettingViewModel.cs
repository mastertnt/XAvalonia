using ReactiveUI;
using XAvalonia.Shell.Abstractions.Shell;

namespace XAvalonia.UserSettings.ViewModels;

/// <summary>Settings editor row for a <see cref="bool"/> or nullable <see cref="bool"/> setting, edited with a check box.</summary>
public sealed class BoolSettingViewModel : SettingViewModel
{
    private bool? mValue;

    /// <summary>Initializes the row and reads the current value.</summary>
    public BoolSettingViewModel(UserSettingDescriptor pSetting, IUserConfiguration pConfiguration, Action<string?> pReportError)
        : base(pSetting, pConfiguration, pReportError)
    {
        IsThreeState = Nullable.GetUnderlyingType(pSetting.ValueType) is not null;
        Refresh();
    }

    /// <summary>Whether the check box accepts an indeterminate (<c>null</c>) state.</summary>
    public bool IsThreeState { get; }

    /// <summary>Check box label: the description, or the title when there is none.</summary>
    public string CheckBoxLabel => Description.Length > 0 ? Description : Title;

    /// <inheritdoc/>
    public override bool ShowDescription => false;

    /// <summary>Current value.</summary>
    public bool? Value
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
        Value = pValue as bool?;
    }
}
