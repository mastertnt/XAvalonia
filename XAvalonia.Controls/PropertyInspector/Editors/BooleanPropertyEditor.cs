using Avalonia.Controls;
using Avalonia.Media;
using XAvalonia.Shell.Abstractions.Properties;

namespace XAvalonia.Controls.Editors;

/// <summary>Check box editor for <see cref="bool"/> and nullable <see cref="bool"/>; the description is its label.</summary>
public sealed class BooleanPropertyEditor : PropertyEditorBase<CheckBox>
{
    /// <inheritdoc/>
    protected override CheckBox CreateControl(IPropertyItem pItem)
    {
        TextBlock lLabel = new TextBlock
        {
            Text = pItem.Description.Length > 0 ? pItem.Description : pItem.DisplayName,
            TextWrapping = TextWrapping.Wrap,
        };
        lLabel.Classes.Add("secondary");

        return new CheckBox
        {
            Content = lLabel,
            IsThreeState = Nullable.GetUnderlyingType(pItem.ValueType) is not null,
        };
    }

    /// <inheritdoc/>
    protected override void UpdateControl(CheckBox pControl, IPropertyItem pItem)
    {
        pControl.IsChecked = pItem.Value as bool?;
    }

    /// <inheritdoc/>
    protected override void ObserveControl(CheckBox pControl, IPropertyItem pItem, Action<object?> pCommit)
    {
        pControl.IsCheckedChanged += (_, _) => pCommit(pControl.IsChecked);
    }
}
