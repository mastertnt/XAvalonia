using XAvalonia.Shell.Abstractions.Properties;

namespace XAvalonia.Controls;

/// <summary>Search and category matching shared by <see cref="PropertyInspector"/> and the views that host it.</summary>
public static class PropertyItemFilter
{
    /// <summary>Splits a search text into terms; every term must match.</summary>
    public static IReadOnlyList<string> SplitTerms(string? pText)
        => (pText ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    /// <summary>Returns <c>true</c> when every term occurs in the label, key, category or description.</summary>
    public static bool Matches(IPropertyItem pItem, IReadOnlyList<string> pTerms)
    {
        foreach (string lTerm in pTerms)
        {
            bool lFound = pItem.DisplayName.Contains(lTerm, StringComparison.CurrentCultureIgnoreCase)
                || pItem.Key.Contains(lTerm, StringComparison.OrdinalIgnoreCase)
                || pItem.Category.Contains(lTerm, StringComparison.CurrentCultureIgnoreCase)
                || pItem.Description.Contains(lTerm, StringComparison.CurrentCultureIgnoreCase);
            if (!lFound)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns <c>true</c> when <paramref name="pCategory"/> is <paramref name="pPath"/> or one of its sub-categories.
    /// An empty or <c>null</c> path matches every category.
    /// </summary>
    public static bool IsInCategory(string pCategory, string? pPath)
    {
        return string.IsNullOrEmpty(pPath)
            || string.Equals(pCategory, pPath, StringComparison.CurrentCultureIgnoreCase)
            || pCategory.StartsWith(pPath + "/", StringComparison.CurrentCultureIgnoreCase);
    }
}
