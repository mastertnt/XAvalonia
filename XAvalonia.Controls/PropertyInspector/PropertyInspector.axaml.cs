using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using XAvalonia.Shell.Abstractions.Properties;

namespace XAvalonia.Controls;

/// <summary>
/// Displays a list of <see cref="IPropertyItem"/> in the style of the Visual Studio Code settings editor:
/// for each item, its category and label, its description, and an editor chosen by value type
/// from <see cref="EditorRegistry"/>. Modified items show an accent bar and a reset button.
/// The list can be filtered by search text, category and "modified only".
/// </summary>
/// <example>
/// <code>
/// &lt;controls:PropertyInspector Items="{Binding Settings}"
///                             FilterText="{Binding SearchText}"
///                             CategoryFilter="{Binding SelectedCategoryPath}"
///                             EditorRegistry="{Binding EditorRegistry}" /&gt;
/// </code>
/// </example>
public partial class PropertyInspector : UserControl
{
    /// <summary>Defines the <see cref="Items"/> property.</summary>
    public static readonly StyledProperty<IEnumerable<IPropertyItem>?> ItemsProperty =
        AvaloniaProperty.Register<PropertyInspector, IEnumerable<IPropertyItem>?>(nameof(Items));

    /// <summary>Defines the <see cref="FilterText"/> property.</summary>
    public static readonly StyledProperty<string?> FilterTextProperty =
        AvaloniaProperty.Register<PropertyInspector, string?>(nameof(FilterText));

    /// <summary>Defines the <see cref="CategoryFilter"/> property.</summary>
    public static readonly StyledProperty<string?> CategoryFilterProperty =
        AvaloniaProperty.Register<PropertyInspector, string?>(nameof(CategoryFilter));

    /// <summary>Defines the <see cref="ShowModifiedOnly"/> property.</summary>
    public static readonly StyledProperty<bool> ShowModifiedOnlyProperty =
        AvaloniaProperty.Register<PropertyInspector, bool>(nameof(ShowModifiedOnly));

    /// <summary>Defines the <see cref="EditorRegistry"/> property.</summary>
    public static readonly StyledProperty<IPropertyEditorRegistry?> EditorRegistryProperty =
        AvaloniaProperty.Register<PropertyInspector, IPropertyEditorRegistry?>(nameof(EditorRegistry));

    /// <summary>Defines the <see cref="EmptyText"/> property.</summary>
    public static readonly StyledProperty<string> EmptyTextProperty =
        AvaloniaProperty.Register<PropertyInspector, string>(nameof(EmptyText), "No properties.");

    /// <summary>Defines the <see cref="VisibleCount"/> property.</summary>
    public static readonly DirectProperty<PropertyInspector, int> VisibleCountProperty =
        AvaloniaProperty.RegisterDirect<PropertyInspector, int>(nameof(VisibleCount), pO => pO.VisibleCount);

    private static readonly IPropertyEditorRegistry DefaultRegistry = new PropertyEditorRegistry();

    private readonly Dictionary<IPropertyItem, PropertyInspectorRow> mRowCache = new Dictionary<IPropertyItem, PropertyInspectorRow>();
    private int mVisibleCount;

    /// <summary>Initializes the control.</summary>
    public PropertyInspector()
    {
        InitializeComponent();
        Refresh();
    }

    /// <summary>Items to display, in order.</summary>
    public IEnumerable<IPropertyItem>? Items
    {
        get => GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>Space-separated search terms; every term must occur in the label, key, category or description.</summary>
    public string? FilterText
    {
        get => GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    /// <summary>Category path to show (with its sub-categories); <c>null</c> or empty shows every category.</summary>
    public string? CategoryFilter
    {
        get => GetValue(CategoryFilterProperty);
        set => SetValue(CategoryFilterProperty, value);
    }

    /// <summary>When <c>true</c>, only modified items are shown.</summary>
    public bool ShowModifiedOnly
    {
        get => GetValue(ShowModifiedOnlyProperty);
        set => SetValue(ShowModifiedOnlyProperty, value);
    }

    /// <summary>Registry used to pick each item's editor; a registry with the built-in editors when <c>null</c>.</summary>
    public IPropertyEditorRegistry? EditorRegistry
    {
        get => GetValue(EditorRegistryProperty);
        set => SetValue(EditorRegistryProperty, value);
    }

    /// <summary>Text shown when <see cref="Items"/> is empty.</summary>
    public string EmptyText
    {
        get => GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    /// <summary>Number of items currently displayed.</summary>
    public int VisibleCount
    {
        get => mVisibleCount;
        private set => SetAndRaise(VisibleCountProperty, ref mVisibleCount, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs pChange)
    {
        base.OnPropertyChanged(pChange);

        if (pChange.Property == ItemsProperty)
        {
            if (pChange.OldValue is INotifyCollectionChanged lOld)
            {
                lOld.CollectionChanged -= OnItemsCollectionChanged;
            }

            if (pChange.NewValue is INotifyCollectionChanged lNew)
            {
                lNew.CollectionChanged += OnItemsCollectionChanged;
            }

            mRowCache.Clear();
            Refresh();
        }
        else if (pChange.Property == EditorRegistryProperty)
        {
            if (pChange.OldValue is IPropertyEditorRegistry lOld)
            {
                lOld.EditorsChanged -= OnEditorsChanged;
            }

            if (pChange.NewValue is IPropertyEditorRegistry lNew)
            {
                lNew.EditorsChanged += OnEditorsChanged;
            }

            mRowCache.Clear();
            Refresh();
        }
        else if (pChange.Property == FilterTextProperty
                 || pChange.Property == CategoryFilterProperty
                 || pChange.Property == ShowModifiedOnlyProperty
                 || pChange.Property == EmptyTextProperty)
        {
            Refresh();
        }
    }

    /// <summary>Re-applies the filters, e.g. after items changed their modified state.</summary>
    public void Refresh()
    {
        // Styled properties set by a XAML parent may arrive before InitializeComponent ran.
        if (PART_Rows is null)
        {
            return;
        }

        IReadOnlyList<string> lTerms = PropertyItemFilter.SplitTerms(FilterText);
        List<IPropertyItem> lAll = Items?.ToList() ?? new List<IPropertyItem>();
        List<PropertyInspectorRow> lRows = lAll
            .Where(pI => (!ShowModifiedOnly || pI.IsModified)
                && PropertyItemFilter.IsInCategory(pI.Category, CategoryFilter)
                && PropertyItemFilter.Matches(pI, lTerms))
            .Select(GetRow)
            .ToList();

        PART_Rows.ItemsSource = lRows;
        VisibleCount = lRows.Count;
        PART_EmptyText.Text = lAll.Count == 0 ? EmptyText : "No properties match the current filters.";
        PART_EmptyText.IsVisible = lRows.Count == 0;
    }

    // Rows (and their editors) are reused while the item and the registry stay the same.
    private PropertyInspectorRow GetRow(IPropertyItem pItem)
    {
        if (!mRowCache.TryGetValue(pItem, out PropertyInspectorRow? lRow))
        {
            IPropertyEditor lEditor = (EditorRegistry ?? DefaultRegistry).Find(pItem.ValueType) ?? PropertyEditorRegistry.Fallback;
            lRow = new PropertyInspectorRow(pItem, lEditor.CreateEditor(pItem));
            mRowCache[pItem] = lRow;
        }

        return lRow;
    }

    private void OnItemsCollectionChanged(object? pSender, NotifyCollectionChangedEventArgs pArgs)
    {
        Refresh();
    }

    private void OnEditorsChanged(object? pSender, EventArgs pArgs)
    {
        mRowCache.Clear();
        Refresh();
    }
}
