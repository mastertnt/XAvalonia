using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using XAvalonia.Bootstrap.Converters;
using XAvalonia.Shell.Abstractions.StatusBar;

namespace XAvalonia.Bootstrap.ViewModels.StatusBar;

/// <summary>View model for a single item in the status bar.</summary>
public partial class StatusBarItemViewModel : ObservableObject
{
    // [ObservableProperty] fields use _ prefix: required by CommunityToolkit.Mvvm source generator.
    [ObservableProperty]
    private string _text = string.Empty;

    internal StatusBarItemViewModel(StatusBarItemContribution pContribution)
    {
        Id    = pContribution.Id;
        Order = pContribution.Order;
        Text  = pContribution.Text;
        Icon  = IconLoader.Load(pContribution.IconUri);
    }

    /// <summary>Unique identifier; used for updates and removal.</summary>
    public string Id { get; }

    /// <summary>Sort order within the item's side of the status bar.</summary>
    public int Order { get; }

    /// <summary>Icon displayed to the left of the text. <c>null</c> means no icon.</summary>
    public Bitmap? Icon { get; }
}
