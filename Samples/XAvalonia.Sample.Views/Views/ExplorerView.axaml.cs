using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace XAvalonia.Sample.Views;

/// <summary>View for the Explorer tool panel.</summary>
public partial class ExplorerView : UserControl
{
    /// <summary>Initializes the view.</summary>
    public ExplorerView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
