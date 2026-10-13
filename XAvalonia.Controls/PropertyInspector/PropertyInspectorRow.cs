using System.Windows.Input;
using Avalonia.Controls;
using XAvalonia.Shell.Abstractions.Properties;

namespace XAvalonia.Controls;

/// <summary>One displayed row of a <see cref="PropertyInspector"/>: the item and the editor created for it.</summary>
public sealed class PropertyInspectorRow
{
    /// <summary>Initializes a row.</summary>
    public PropertyInspectorRow(IPropertyItem pItem, Control pEditor)
    {
        Item         = pItem;
        Editor       = pEditor;
        ResetCommand = new ResetItemCommand(pItem);
    }

    /// <summary>Restores the default value of the item.</summary>
    public ICommand ResetCommand { get; }

    /// <summary>The displayed property.</summary>
    public IPropertyItem Item { get; }

    /// <summary>Editor control created by the registry.</summary>
    public Control Editor { get; }

    /// <summary>Category path shown before the label (e.g. <c>Text Editor › Font</c>).</summary>
    public string Breadcrumb => Item.Category.Replace("/", " › ", StringComparison.Ordinal);

    /// <summary>Whether the help text is shown above the editor; check-box editors show it as their label.</summary>
    public bool ShowDescription => Item.Description.Length > 0 && Editor is not CheckBox;

    private sealed class ResetItemCommand : ICommand
    {
        private readonly IPropertyItem mItem;

        public ResetItemCommand(IPropertyItem pItem)
        {
            mItem = pItem;
        }

        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? pParameter) => !mItem.IsReadOnly;

        public void Execute(object? pParameter) => mItem.Reset();
    }
}
