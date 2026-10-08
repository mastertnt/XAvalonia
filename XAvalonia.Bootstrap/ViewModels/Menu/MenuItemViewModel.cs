using System.Windows.Input;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using XAvalonia.Bootstrap.Converters;
using XAvalonia.Shell.Abstractions.Menus;

namespace XAvalonia.Bootstrap.ViewModels.Menu;

/// <summary>View model for a single item (or separator) within a top-level menu.</summary>
public class MenuItemViewModel
{
    internal MenuItemViewModel(MenuItemContribution pContribution)
    {
        Id    = pContribution.Id;
        Order = pContribution.Order;

        if (pContribution is MenuSeparatorContribution)
        {
            IsSeparator = true;
            Header      = string.Empty;
        }
        else
        {
            Header       = pContribution.Header;
            Command      = pContribution.Command;
            InputGesture = pContribution.InputGesture is null
                ? null
                : KeyGesture.Parse(pContribution.InputGesture);
            Icon = IconLoader.Load(pContribution.IconUri);
        }
    }

    /// <summary>Unique identifier within the parent menu.</summary>
    public string Id { get; }

    /// <summary>Display text (supports access-key underscore).</summary>
    public string Header { get; }

    /// <summary>Command invoked when the item is clicked.</summary>
    public ICommand? Command { get; }

    /// <summary>Keyboard shortcut shown on the right side of the item.</summary>
    public KeyGesture? InputGesture { get; }

    /// <summary>Whether this item renders as a horizontal separator line.</summary>
    public bool IsSeparator { get; }

    /// <summary>Sort order within the parent menu.</summary>
    public int Order { get; }

    /// <summary>Optional icon displayed to the left of the label.</summary>
    public Bitmap? Icon { get; }
}
