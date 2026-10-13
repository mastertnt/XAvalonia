using Avalonia.Controls;

namespace XAvalonia.Shell.Abstractions.Properties;

/// <summary>
/// Creates the control that edits an <see cref="IPropertyItem"/> value.
/// Registered for a value type in the <see cref="IPropertyEditorRegistry"/>.
/// </summary>
public interface IPropertyEditor
{
    /// <summary>
    /// Returns a new control bound to <paramref name="pItem"/>: it shows <see cref="IPropertyItem.Value"/>,
    /// follows its changes, and writes edits back to it. Called once per displayed row.
    /// </summary>
    Control CreateEditor(IPropertyItem pItem);
}
