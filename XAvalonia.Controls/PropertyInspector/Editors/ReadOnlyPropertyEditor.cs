using Avalonia.Controls;
using XAvalonia.Shell.Abstractions.Properties;

namespace XAvalonia.Controls.Editors;

/// <summary>Fallback editor for types without a registered editor: shows the value as text.</summary>
public sealed class ReadOnlyPropertyEditor : PropertyEditorBase<TextBlock>
{
    /// <inheritdoc/>
    protected override TextBlock CreateControl(IPropertyItem pItem) => new TextBlock();

    /// <inheritdoc/>
    protected override void UpdateControl(TextBlock pControl, IPropertyItem pItem)
    {
        pControl.Text = pItem.Value?.ToString() ?? "(none)";
    }

    /// <inheritdoc/>
    protected override void ObserveControl(TextBlock pControl, IPropertyItem pItem, Action<object?> pCommit)
    {
    }
}
