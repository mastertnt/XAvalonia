using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace XAvalonia.Sample.Views;

/// <summary>View for the Output tool panel.</summary>
public partial class OutputView : UserControl
{
    /// <summary>Initializes the view.</summary>
    public OutputView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
