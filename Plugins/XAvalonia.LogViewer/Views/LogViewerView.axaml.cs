using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace XAvalonia.LogViewer;

/// <summary>View for the Log Viewer tool panel.</summary>
public partial class LogViewerView : UserControl
{
    private LogViewerViewModel? mCurrentViewModel;

    /// <summary>Initializes the view.</summary>
    public LogViewerView()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc/>
    protected override void OnDataContextChanged(EventArgs pArgs)
    {
        base.OnDataContextChanged(pArgs);

        if (mCurrentViewModel is not null)
        {
            mCurrentViewModel.FilteredEntries.CollectionChanged -= OnEntriesChanged;
            mCurrentViewModel = null;
        }

        if (DataContext is LogViewerViewModel lVm)
        {
            mCurrentViewModel = lVm;
            lVm.FilteredEntries.CollectionChanged += OnEntriesChanged;
        }
    }

    private void OnEntriesChanged(object? pSender, NotifyCollectionChangedEventArgs pArgs)
    {
        ListBox? lList = this.FindControl<ListBox>("LogList");
        if (lList is null || lList.ItemCount == 0)
        {
            return;
        }

        lList.ScrollIntoView(lList.ItemCount - 1);
    }
}
