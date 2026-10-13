using System.ComponentModel;

namespace XAvalonia.Shell.Abstractions.Properties;

/// <summary>
/// One editable property displayed by a property inspector: label, help text, category, typed value
/// and "modified" state. Implementations raise <see cref="INotifyPropertyChanged.PropertyChanged"/>
/// for <see cref="Value"/> and <see cref="IsModified"/> so that editors stay in sync.
/// </summary>
public interface IPropertyItem : INotifyPropertyChanged
{
    /// <summary>Stable identifier of the property (e.g. <c>Section.key</c>), shown as a tooltip and searchable.</summary>
    string Key { get; }

    /// <summary>Label shown in bold.</summary>
    string DisplayName { get; }

    /// <summary>Help text shown under the label; may be empty.</summary>
    string Description { get; }

    /// <summary>Category path, segments separated by <c>/</c> (e.g. <c>Text Editor/Font</c>).</summary>
    string Category { get; }

    /// <summary>CLR type of <see cref="Value"/>; selects the editor in the <see cref="IPropertyEditorRegistry"/>.</summary>
    Type ValueType { get; }

    /// <summary>
    /// Current value. Setting it applies the change; implementations report failures
    /// (validation, persistence) themselves rather than throwing to the editor.
    /// </summary>
    object? Value { get; set; }

    /// <summary>Whether the value differs from its default; shows the modified marker and the reset button.</summary>
    bool IsModified { get; }

    /// <summary>Whether editing is disabled.</summary>
    bool IsReadOnly { get; }

    /// <summary>Smallest accepted numeric value, or <c>null</c> when unbounded.</summary>
    double? Minimum { get; }

    /// <summary>Largest accepted numeric value, or <c>null</c> when unbounded.</summary>
    double? Maximum { get; }

    /// <summary>Restores the default value.</summary>
    void Reset();
}
