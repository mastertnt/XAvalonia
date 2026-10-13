using Avalonia.Controls;
using Avalonia.Layout;
using XAvalonia.Shell.Abstractions.Properties;

namespace XAvalonia.Controls.Editors;

/// <summary>Numeric up-down editor for <see cref="int"/> and <see cref="double"/> (and their nullable forms).</summary>
public sealed class NumericPropertyEditor : PropertyEditorBase<NumericUpDown>
{
    /// <inheritdoc/>
    protected override NumericUpDown CreateControl(IPropertyItem pItem)
    {
        Type lType = Nullable.GetUnderlyingType(pItem.ValueType) ?? pItem.ValueType;
        bool lIsInteger = lType == typeof(int);

        return new NumericUpDown
        {
            Minimum = ToDecimal(pItem.Minimum) ?? decimal.MinValue,
            Maximum = ToDecimal(pItem.Maximum) ?? decimal.MaxValue,
            Increment = 1,
            FormatString = lIsInteger ? "0" : "0.###",
            Width = 180,
            HorizontalAlignment = HorizontalAlignment.Left,
        };
    }

    /// <inheritdoc/>
    protected override void UpdateControl(NumericUpDown pControl, IPropertyItem pItem)
    {
        pControl.Value = pItem.Value switch
        {
            int lInt       => lInt,
            double lDouble => ToDecimal(lDouble),
            _              => null,
        };
    }

    /// <inheritdoc/>
    protected override void ObserveControl(NumericUpDown pControl, IPropertyItem pItem, Action<object?> pCommit)
    {
        Type lType = Nullable.GetUnderlyingType(pItem.ValueType) ?? pItem.ValueType;
        pControl.ValueChanged += (_, pArgs) =>
        {
            if (pArgs.NewValue is not decimal lValue)
            {
                pCommit(null);
            }
            else if (lType == typeof(int))
            {
                pCommit((int)Math.Round(lValue));
            }
            else
            {
                pCommit((double)lValue);
            }
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
