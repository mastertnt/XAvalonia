using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;
using XAvalonia.Shell.Abstractions.Shell;

namespace XAvalonia.UserSettings.ViewModels;

/// <summary>
/// View model for the settings editor document tab: a category tree, a search box,
/// and the list of matching settings, each with an editor bound to <see cref="IUserConfiguration"/>.
/// </summary>
public sealed class UserSettingsViewModel : Document
{
    private readonly IUserConfiguration mConfiguration;
    private readonly List<SettingViewModel> mAllSettings;
    private readonly SettingCategoryViewModel mAllNode;
    private SettingCategoryViewModel? mSelectedCategory;
    private string mSearchText = string.Empty;
    private bool mShowModifiedOnly;
    private string? mErrorMessage;
    private string mResultSummary = string.Empty;

    /// <summary>Initializes the editor from the settings currently declared by plugins.</summary>
    /// <param name="pConfiguration">The user configuration to display and edit.</param>
    public UserSettingsViewModel(IUserConfiguration pConfiguration)
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
        mAllSettings = pConfiguration.Settings
            .Select(CreateRow)
            .OfType<SettingViewModel>()
            .ToList();

        mAllNode = new SettingCategoryViewModel("All Settings", string.Empty);
        Categories = BuildCategoryTree(mAllNode, mAllSettings);
        mSelectedCategory = mAllNode;

        ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);

        mConfiguration.SettingChanged += OnSettingChanged;
        ApplyFilter();
    }

    /// <summary>Root nodes of the category tree; the first one shows every setting.</summary>
    public ObservableCollection<SettingCategoryViewModel> Categories { get; }

    /// <summary>Settings matching the selected category, the search text and the "modified" filter.</summary>
    public ObservableCollection<SettingViewModel> VisibleSettings { get; } = new ObservableCollection<SettingViewModel>();

    /// <summary>Number of settings declared by plugins.</summary>
    public int TotalCount => mAllSettings.Count;

    /// <summary>Full path of the user configuration file.</summary>
    public string FilePath => mConfiguration.FilePath;

    /// <summary>Clears the search box.</summary>
    public ICommand ClearSearchCommand { get; }

    /// <summary>Selected category; <c>null</c> behaves like "All Settings".</summary>
    public SettingCategoryViewModel? SelectedCategory
    {
        get => mSelectedCategory;
        set
        {
            this.RaiseAndSetIfChanged(ref mSelectedCategory, value);
            ApplyFilter();
        }
    }

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

    /// <summary>Summary shown above the list (e.g. <c>3 settings found</c>).</summary>
    public string ResultSummary
    {
        get => mResultSummary;
        private set => this.RaiseAndSetIfChanged(ref mResultSummary, value);
    }

    /// <summary>Message shown when the list is empty.</summary>
    public string EmptyMessage => mAllSettings.Count == 0
        ? "No settings available. Plugins declare settings with [UserConfiguration]."
        : "No settings match the current filters.";

    /// <summary>Whether no setting matches the current filters.</summary>
    public bool HasNoResults => VisibleSettings.Count == 0;

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

    private SettingViewModel? CreateRow(UserSettingDescriptor pSetting)
    {
        Type lType = Nullable.GetUnderlyingType(pSetting.ValueType) ?? pSetting.ValueType;
        Action<string?> lReportError = pMessage => ErrorMessage = pMessage;

        if (lType == typeof(bool))                         return new BoolSettingViewModel(pSetting, mConfiguration, lReportError);
        if (lType == typeof(int) || lType == typeof(double)) return new NumberSettingViewModel(pSetting, mConfiguration, lReportError);
        if (lType == typeof(string))                       return new TextSettingViewModel(pSetting, mConfiguration, lReportError);
        if (lType.IsEnum)                                  return new ChoiceSettingViewModel(pSetting, mConfiguration, lReportError);
        return null;
    }

    // Builds "All Settings" followed by one root per first category segment, nested by '/'.
    private static ObservableCollection<SettingCategoryViewModel> BuildCategoryTree(
        SettingCategoryViewModel pAllNode, IEnumerable<SettingViewModel> pSettings)
    {
        ObservableCollection<SettingCategoryViewModel> lRoots = new ObservableCollection<SettingCategoryViewModel> { pAllNode };
        ObservableCollection<SettingCategoryViewModel> lCategories = new ObservableCollection<SettingCategoryViewModel>();

        foreach (string lCategory in pSettings.Select(pS => pS.Setting.Category).Distinct(StringComparer.CurrentCultureIgnoreCase))
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

    private void ApplyFilter()
    {
        string[] lTerms = mSearchText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        List<SettingViewModel> lMatches = mAllSettings
            .Where(pS => (!mShowModifiedOnly || pS.IsModified) && pS.Matches(lTerms))
            .ToList();

        foreach (SettingCategoryViewModel lNode in Categories.SelectMany(pN => pN.SelfAndDescendants()))
        {
            lNode.MatchCount = lMatches.Count(pS => lNode.Contains(pS.Setting.Category));
        }

        // A search that leaves the selected category empty falls back to "All Settings", as in VS Code.
        if (mSelectedCategory is null || !mSelectedCategory.HasMatches)
        {
            mSelectedCategory = mAllNode;
            this.RaisePropertyChanged(nameof(SelectedCategory));
        }

        SettingCategoryViewModel lCategory = mSelectedCategory;
        VisibleSettings.Clear();
        foreach (SettingViewModel lSetting in lMatches.Where(pS => lCategory.Contains(pS.Setting.Category)))
        {
            VisibleSettings.Add(lSetting);
        }

        ResultSummary = VisibleSettings.Count switch
        {
            0 => "No settings found",
            1 => "1 setting",
            _ => $"{VisibleSettings.Count} settings",
        };
        this.RaisePropertyChanged(nameof(HasNoResults));
    }

    // Keeps rows in sync when a setting is changed elsewhere (another editor, a plugin, a reset).
    private void OnSettingChanged(object? pSender, UserSettingChangedEventArgs pArgs)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => OnSettingChanged(pSender, pArgs));
            return;
        }

        mAllSettings.FirstOrDefault(pS => ReferenceEquals(pS.Setting, pArgs.Setting))?.Refresh();
    }
}
