using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace XAvalonia.Sample.Views;

/// <summary>View model for the Properties tool panel (right side).</summary>
public sealed class PropertiesViewModel : Tool
{
    private string mSelectedItem = "(aucun)";

    /// <summary>Initializes the properties panel.</summary>
    public PropertiesViewModel()
    {
        Id    = "properties";
        Title = "Propriétés";
        DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanPin   = true,
            CanFloat = true,
            CanClose = true
        };
    }

    /// <summary>Display name of the currently selected item.</summary>
    public string SelectedItem
    {
        get => mSelectedItem;
        set => this.RaiseAndSetIfChanged(ref mSelectedItem, value);
    }
}
