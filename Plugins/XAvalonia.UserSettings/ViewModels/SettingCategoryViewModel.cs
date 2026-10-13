using System.Collections.ObjectModel;
using ReactiveUI;
using XAvalonia.Controls;

namespace XAvalonia.UserSettings.ViewModels;

/// <summary>Node of the category tree shown on the left of the settings editor.</summary>
public sealed class SettingCategoryViewModel : ReactiveObject
{
    private int mMatchCount;

    /// <summary>Initializes a category node.</summary>
    /// <param name="pName">Label of the node (last segment of the path).</param>
    /// <param name="pPath">Full category path, segments separated by <c>/</c>; empty for the "all settings" node.</param>
    public SettingCategoryViewModel(string pName, string pPath)
    {
        Name = pName;
        Path = pPath;
    }

    /// <summary>Label of the node.</summary>
    public string Name { get; }

    /// <summary>Full category path; empty for the "all settings" node.</summary>
    public string Path { get; }

    /// <summary>Child categories.</summary>
    public ObservableCollection<SettingCategoryViewModel> Children { get; } = new ObservableCollection<SettingCategoryViewModel>();

    /// <summary>Number of settings in this category and its children that match the current search.</summary>
    public int MatchCount
    {
        get => mMatchCount;
        set
        {
            this.RaiseAndSetIfChanged(ref mMatchCount, value);
            this.RaisePropertyChanged(nameof(HasMatches));
        }
    }

    /// <summary>Whether the node is shown; categories without matches are hidden while searching.</summary>
    public bool HasMatches => mMatchCount > 0 || Path.Length == 0;

    /// <summary>Returns <c>true</c> when <paramref name="pCategory"/> is this category or one of its children.</summary>
    public bool Contains(string pCategory)
    {
        return PropertyItemFilter.IsInCategory(pCategory, Path);
    }

    /// <summary>Returns this node and all its descendants.</summary>
    public IEnumerable<SettingCategoryViewModel> SelfAndDescendants()
    {
        yield return this;
        foreach (SettingCategoryViewModel lChild in Children)
        {
            foreach (SettingCategoryViewModel lNode in lChild.SelfAndDescendants())
            {
                yield return lNode;
            }
        }
    }
}
