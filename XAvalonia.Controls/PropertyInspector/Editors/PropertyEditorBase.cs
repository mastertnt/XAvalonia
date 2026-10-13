using System.ComponentModel;
using Avalonia.Controls;
using XAvalonia.Shell.Abstractions.Properties;

namespace XAvalonia.Controls.Editors;

/// <summary>
/// Base class for editors: keeps a control and an <see cref="IPropertyItem"/> in sync in both directions.
/// The item is observed only while the control is in the visual tree, so discarded rows do not leak.
/// </summary>
/// <typeparam name="TControl">Type of the editor control.</typeparam>
public abstract class PropertyEditorBase<TControl> : IPropertyEditor where TControl : Control
{
    /// <inheritdoc/>
    public Control CreateEditor(IPropertyItem pItem)
    {
        TControl lControl = CreateControl(pItem);
        bool lIsUpdating = false;

        void PushValue()
        {
            lIsUpdating = true;
            try
            {
                UpdateControl(lControl, pItem);
            }
            finally
            {
                lIsUpdating = false;
            }
        }

        void OnItemChanged(object? pSender, PropertyChangedEventArgs pArgs)
        {
            if (string.IsNullOrEmpty(pArgs.PropertyName) || pArgs.PropertyName == nameof(IPropertyItem.Value))
            {
                PushValue();
            }
        }

        lControl.AttachedToVisualTree += (_, _) =>
        {
            pItem.PropertyChanged += OnItemChanged;
            PushValue();
        };
        lControl.DetachedFromVisualTree += (_, _) => pItem.PropertyChanged -= OnItemChanged;

        PushValue();
        ObserveControl(lControl, pItem, pValue =>
        {
            if (!lIsUpdating)
            {
                pItem.Value = pValue;
            }
        });
        lControl.IsEnabled = !pItem.IsReadOnly;
        return lControl;
    }

    /// <summary>Creates the editor control for <paramref name="pItem"/>.</summary>
    protected abstract TControl CreateControl(IPropertyItem pItem);

    /// <summary>Shows <see cref="IPropertyItem.Value"/> in the control; edits made here are not written back.</summary>
    protected abstract void UpdateControl(TControl pControl, IPropertyItem pItem);

    /// <summary>Subscribes to the control's edits and passes each new value to <paramref name="pCommit"/>.</summary>
    protected abstract void ObserveControl(TControl pControl, IPropertyItem pItem, Action<object?> pCommit);
}
