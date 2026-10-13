using ReactiveUI;
using XAvalonia.Shell.Abstractions.Shell;

namespace XAvalonia.UserSettings.ViewModels;

/// <summary>Settings editor row for an <see cref="int"/> or <see cref="double"/> setting, edited with a numeric up-down.</summary>
public sealed class NumberSettingViewModel : SettingViewModel
{
    private decimal? mValue;

    /// <summary>Initializes the row and reads the current value.</summary>
    public NumberSettingViewModel(UserSettingDescriptor pSetting, IUserConfiguration pConfiguration, Action<string?> pReportError)
        : base(pSetting, pConfiguration, pReportError)
    {
        Type lType = Nullable.GetUnderlyingType(pSetting.ValueType) ?? pSetting.ValueType;
        IsInteger    = lType == typeof(int);
        FormatString = IsInteger ? "0" : "0.###";
        Minimum      = ToDecimal(pSetting.Minimum) ?? decimal.MinValue;
        Maximum      = ToDecimal(pSetting.Maximum) ?? decimal.MaxValue;
        Refresh();
    }

    /// <summary>Whether the setting only accepts whole numbers.</summary>
    public bool IsInteger { get; }

    /// <summary>Display format of the numeric up-down.</summary>
    public string FormatString { get; }

    /// <summary>Smallest accepted value.</summary>
    public decimal Minimum { get; }

    /// <summary>Largest accepted value.</summary>
    public decimal Maximum { get; }

    /// <summary>Current value; <c>null</c> only for nullable settings.</summary>
    public decimal? Value
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
        Value = pValue switch
        {
            int lInt       => lInt,
            double lDouble => ToDecimal(lDouble),
            _              => null,
        };
    }

    private static decimal? ToDecimal(double? pValue)
    {
        if (pValue is null || double.IsNaN(pValue.Value))
        {
            return null;
        }

        return pValue.Value >= (double)decimal.MaxValue ? decimal.MaxValue
            : pValue.Value <= (double)decimal.MinValue ? decimal.MinValue
            : (decimal)pValue.Value;
    }
}
