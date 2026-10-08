using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace XAvalonia.Sample.Views;

/// <summary>View for the generic document tab.</summary>
public partial class DocumentView : UserControl
{
    /// <summary>Initializes the view.</summary>
    public DocumentView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
