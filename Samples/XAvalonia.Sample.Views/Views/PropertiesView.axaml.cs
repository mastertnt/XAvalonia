using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace XAvalonia.Sample.Views;

/// <summary>View for the Properties tool panel.</summary>
public partial class PropertiesView : UserControl
{
    /// <summary>Initializes the view.</summary>
    public PropertiesView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
