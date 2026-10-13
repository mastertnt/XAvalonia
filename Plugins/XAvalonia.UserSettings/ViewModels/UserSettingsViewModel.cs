using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;
using XAvalonia.Controls;
using XAvalonia.Shell.Abstractions.Properties;
using XAvalonia.Shell.Abstractions.Shell;

namespace XAvalonia.UserSettings.ViewModels;

/// <summary>
/// View model for the settings editor document tab: a category tree, a search box and the
/// settings themselves, displayed and edited by a <c>PropertyInspector</c>.
/// </summary>
public sealed class UserSettingsViewModel : Document
{
    private readonly IUserConfiguration mConfiguration;
    private readonly List<UserSettingItem> mItems;
    private readonly SettingCategoryViewModel mAllNode;
    private SettingCategoryViewModel? mSelectedCategory;
    private string mSearchText = string.Empty;
    private bool mShowModifiedOnly;
    private string? mErrorMessage;
    private string mResultSummary = string.Empty;

    /// <summary>Initializes the editor from the settings currently declared by plugins.</summary>
    /// <param name="pConfiguration">The user configuration to display and edit.</param>
    /// <param name="pEditorRegistry">Editors used by the inspector; <c>null</c> for the built-in ones.</param>
    public UserSettingsViewModel(IUserConfiguration pConfiguration, IPropertyEditorRegistry? pEditorRegistry)
    {
        Id    = "user-settings";
        Title = "Settings";
        DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanPin   = true,
            CanFloat = true,
            CanClose = true
        };

        mConfiguration = pConfiguration;
        EditorRegistry = pEditorRegistry;
        mItems = pConfiguration.Settings
            .Select(pS => new UserSettingItem(pS, pConfiguration, pMessage => ErrorMessage = pMessage))
            .ToList();
        Items = mItems;

        mAllNode = new SettingCategoryViewModel("All Settings", string.Empty);
        Categories = BuildCategoryTree(mAllNode, mItems.Select(pI => pI.Category));
        mSelectedCategory = mAllNode;

        ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);

        mConfiguration.SettingChanged += OnSettingChanged;
        ApplyFilter();
    }

    /// <summary>Settings shown by the inspector.</summary>
    public IReadOnlyList<IPropertyItem> Items { get; }

    /// <summary>Editors used by the inspector; <c>null</c> for the built-in ones.</summary>
    public IPropertyEditorRegistry? EditorRegistry { get; }

    /// <summary>Root nodes of the category tree; the first one shows every setting.</summary>
    public ObservableCollection<SettingCategoryViewModel> Categories { get; }

    /// <summary>Number of settings declared by plugins.</summary>
    public int TotalCount => mItems.Count;

    /// <summary>Full path of the user configuration file.</summary>
    public string FilePath => mConfiguration.FilePath;

    /// <summary>Clears the search box.</summary>
    public ICommand ClearSearchCommand { get; }

    /// <summary>Selected category in the tree.</summary>
    public SettingCategoryViewModel? SelectedCategory
    {
        get => mSelectedCategory;
        set
        {
            // The tree clears its selection while it is detached (e.g. when switching tabs): keep ours,
            // otherwise re-selecting "All Settings" would feed back into the tree endlessly.
            if (value is null || ReferenceEquals(value, mSelectedCategory))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref mSelectedCategory, value);
            ApplyFilter();
        }
    }

    /// <summary>Path of the selected category, passed to the inspector; empty for "All Settings".</summary>
    public string SelectedCategoryPath => mSelectedCategory?.Path ?? string.Empty;

    /// <summary>Space-separated search terms; every term must match.</summary>
    public string SearchText
    {
        get => mSearchText;
        set
        {
            this.RaiseAndSetIfChanged(ref mSearchText, value ?? string.Empty);
            this.RaisePropertyChanged(nameof(HasSearchText));
            ApplyFilter();
        }
    }

    /// <summary>Whether the search box contains text.</summary>
    public bool HasSearchText => mSearchText.Length > 0;

    /// <summary>When <c>true</c>, only settings explicitly set in the configuration file are listed.</summary>
    public bool ShowModifiedOnly
    {
        get => mShowModifiedOnly;
        set
        {
            this.RaiseAndSetIfChanged(ref mShowModifiedOnly, value);
            ApplyFilter();
        }
    }

    /// <summary>Summary shown next to the search box (e.g. <c>3 settings</c>).</summary>
    public string ResultSummary
    {
        get => mResultSummary;
        private set => this.RaiseAndSetIfChanged(ref mResultSummary, value);
    }

    /// <summary>Last error raised while saving a setting, or <c>null</c>.</summary>
    public string? ErrorMessage
    {
        get => mErrorMessage;
        private set
        {
            this.RaiseAndSetIfChanged(ref mErrorMessage, value);
            this.RaisePropertyChanged(nameof(HasError));
        }
    }

    /// <summary>Whether <see cref="ErrorMessage"/> is set.</summary>
    public bool HasError => mErrorMessage is not null;

    /// <summary>Stops listening to configuration changes; call when the document is closed.</summary>
    public void Detach()
    {
        mConfiguration.SettingChanged -= OnSettingChanged;
    }

    // Builds "All Settings" followed by one root per first category segment, nested by '/'.
    private static ObservableCollection<SettingCategoryViewModel> BuildCategoryTree(
        SettingCategoryViewModel pAllNode, IEnumerable<string> pCategories)
    {
        ObservableCollection<SettingCategoryViewModel> lRoots = new ObservableCollection<SettingCategoryViewModel> { pAllNode };
        ObservableCollection<SettingCategoryViewModel> lCategories = new ObservableCollection<SettingCategoryViewModel>();

        foreach (string lCategory in pCategories.Distinct(StringComparer.CurrentCultureIgnoreCase))
        {
            ObservableCollection<SettingCategoryViewModel> lLevel = lCategories;
            string lPath = string.Empty;

            foreach (string lSegment in lCategory.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                lPath = lPath.Length == 0 ? lSegment : lPath + "/" + lSegment;
                SettingCategoryViewModel? lNode = lLevel.FirstOrDefault(pN => string.Equals(pN.Name, lSegment, StringComparison.CurrentCultureIgnoreCase));
                if (lNode is null)
                {
                    lNode = new SettingCategoryViewModel(lSegment, lPath);
                    lLevel.Add(lNode);
                }

                lLevel = lNode.Children;
            }
        }

        foreach (SettingCategoryViewModel lNode in lCategories.OrderBy(pN => pN.Name, StringComparer.CurrentCultureIgnoreCase))
        {
            lRoots.Add(lNode);
        }

        return lRoots;
    }

    // Updates the per-category counts and the summary; the inspector applies the same filters to its rows.
    private void ApplyFilter()
    {
        IReadOnlyList<string> lTerms = PropertyItemFilter.SplitTerms(mSearchText);
        List<UserSettingItem> lMatches = mItems
            .Where(pI => (!mShowModifiedOnly || pI.IsModified) && PropertyItemFilter.Matches(pI, lTerms))
            .ToList();

        foreach (SettingCategoryViewModel lNode in Categories.SelectMany(pN => pN.SelfAndDescendants()))
        {
            lNode.MatchCount = lMatches.Count(pI => lNode.Contains(pI.Category));
        }

        // A search that leaves the selected category empty falls back to "All Settings", as in VS Code.
        if (mSelectedCategory is null || (!mSelectedCategory.HasMatches && !ReferenceEquals(mSelectedCategory, mAllNode)))
        {
            mSelectedCategory = mAllNode;
            this.RaisePropertyChanged(nameof(SelectedCategory));
        }

        this.RaisePropertyChanged(nameof(SelectedCategoryPath));

        SettingCategoryViewModel lCategory = mSelectedCategory;
        int lVisible = lMatches.Count(pI => lCategory.Contains(pI.Category));
        ResultSummary = lVisible switch
        {
            0 => "No settings found",
            1 => "1 setting",
            _ => $"{lVisible} settings",
        };
    }

    // Keeps items in sync when a setting is changed elsewhere (another editor, a plugin, a reset).
    private void OnSettingChanged(object? pSender, UserSettingChangedEventArgs pArgs)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnSettingChanged(pSender, pArgs));
            return;
        }

        mItems.FirstOrDefault(pI => ReferenceEquals(pI.Setting, pArgs.Setting))?.Refresh();
    }
}
