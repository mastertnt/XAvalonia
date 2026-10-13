using Avalonia.Controls;
using XAvalonia.Shell.Abstractions.Properties;

namespace XAvalonia.Controls.Editors;

/// <summary>Combo box editor listing the values of an enum, in declaration order.</summary>
public sealed class EnumPropertyEditor : PropertyEditorBase<ComboBox>
{
    /// <inheritdoc/>
    protected override ComboBox CreateControl(IPropertyItem pItem)
    {
        Type lEnumType = Nullable.GetUnderlyingType(pItem.ValueType) ?? pItem.ValueType;
        return new ComboBox
        {
            ItemsSource = Enum.GetValues(lEnumType).Cast<object>().ToList(),
            MinWidth = 180,
        };
    }

    /// <inheritdoc/>
    protected override void UpdateControl(ComboBox pControl, IPropertyItem pItem)
    {
        pControl.SelectedItem = pItem.Value;
    }

    /// <inheritdoc/>
    protected override void ObserveControl(ComboBox pControl, IPropertyItem pItem, Action<object?> pCommit)
    {
        pControl.SelectionChanged += (_, _) =>
        {
            if (pControl.SelectedItem is not null)
            {
                pCommit(pControl.SelectedItem);
            }
        };
    }
}
