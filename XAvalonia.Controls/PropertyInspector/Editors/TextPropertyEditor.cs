using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using XAvalonia.Shell.Abstractions.Properties;

namespace XAvalonia.Controls.Editors;

/// <summary>Text box editor for <see cref="string"/>; the value is committed on Enter or when the focus leaves.</summary>
public sealed class TextPropertyEditor : PropertyEditorBase<TextBox>
{
    /// <inheritdoc/>
    protected override TextBox CreateControl(IPropertyItem pItem)
    {
        return new TextBox
        {
            MaxWidth = 480,
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
    }

    /// <inheritdoc/>
    protected override void UpdateControl(TextBox pControl, IPropertyItem pItem)
    {
        pControl.Text = pItem.Value as string;
    }

    /// <inheritdoc/>
    protected override void ObserveControl(TextBox pControl, IPropertyItem pItem, Action<object?> pCommit)
    {
        void Commit()
        {
            if (!Equals(pControl.Text ?? string.Empty, pItem.Value as string ?? string.Empty))
            {
                pCommit(pControl.Text);
            }
        }

        pControl.LostFocus += (_, _) => Commit();
        pControl.KeyDown += (_, pArgs) =>
        {
            if (pArgs.Key == Key.Enter)
            {
                Commit();
            }
        };
    }
}
