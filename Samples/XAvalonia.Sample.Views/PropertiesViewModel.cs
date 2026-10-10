using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;
using XAvalonia.Shell.Abstractions.Selection;

namespace XAvalonia.Sample.Views;

/// <summary>View model for the Properties tool panel (right side).</summary>
public sealed class PropertiesViewModel : Tool
{
    private string mSelectedItem = "(aucun)";

    /// <summary>Initializes the properties panel, which follows the active selection.</summary>
    /// <param name="pSelectionManager">Source of the active selection.</param>
    public PropertiesViewModel(ISelectionManager pSelectionManager)
    {
        Id    = "properties";
        Title = "Propriétés";
        DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanPin   = true,
            CanFloat = true,
            CanClose = true
        };

        pSelectionManager.ActiveSelectionChanged += (_, pArgs) =>
            SelectedItem = pArgs.Context.PrimaryItem is FileNode lNode ? lNode.Name : "(aucun)";
    }

    /// <summary>Display name of the currently selected item.</summary>
    public string SelectedItem
    {
        get => mSelectedItem;
        set => this.RaiseAndSetIfChanged(ref mSelectedItem, value);
    }
}
