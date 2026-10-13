using ReactiveUI;
using XAvalonia.Shell.Abstractions.Shell;

namespace XAvalonia.UserSettings.ViewModels;

/// <summary>Settings editor row for an enum setting, edited with a combo box.</summary>
public sealed class ChoiceSettingViewModel : SettingViewModel
{
    private object? mValue;

    /// <summary>Initializes the row and reads the current value.</summary>
    public ChoiceSettingViewModel(UserSettingDescriptor pSetting, IUserConfiguration pConfiguration, Action<string?> pReportError)
        : base(pSetting, pConfiguration, pReportError)
    {
        Type lEnumType = Nullable.GetUnderlyingType(pSetting.ValueType) ?? pSetting.ValueType;
        Choices = Enum.GetValues(lEnumType).Cast<object>().ToList();
        Refresh();
    }

    /// <summary>Accepted values, in declaration order.</summary>
    public IReadOnlyList<object> Choices { get; }

    /// <summary>Current value.</summary>
    public object? Value
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
        Value = pValue;
    }
}
