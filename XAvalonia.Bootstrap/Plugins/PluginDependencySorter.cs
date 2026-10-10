using System.Reflection;
using XAvalonia.Shell.Abstractions.Plugins;

namespace XAvalonia.Bootstrap.Plugins;

/// <summary>
/// Orders plugins so that every plugin comes after the plugins it depends on.
/// Dependencies are read from <see cref="DependsOnPluginAttribute"/> and
/// <see cref="DependsOnServiceAttribute"/> on the plugin class.
/// </summary>
public static class PluginDependencySorter
{
    /// <summary>
    /// Returns <paramref name="pPlugins"/> in dependency order.
    /// Plugins with no constraint between them keep their relative input order.
    /// </summary>
    /// <param name="pPlugins">Plugins to order.</param>
    /// <returns>A new list where each plugin follows all of its dependencies.</returns>
    /// <exception cref="PluginDependencyException">
    /// Two plugins share an <see cref="IPlugin.Id"/>, a required plugin dependency is missing,
    /// or the dependencies form a cycle.
    /// </exception>
    public static IReadOnlyList<IPlugin> Sort(IReadOnlyList<IPlugin> pPlugins)
    {
        Dictionary<string, int> lIndexById = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int lIndex = 0; lIndex < pPlugins.Count; lIndex++)
        {
            IPlugin lPlugin = pPlugins[lIndex];
            if (!lIndexById.TryAdd(lPlugin.Id, lIndex))
            {
                IPlugin lOther = pPlugins[lIndexById[lPlugin.Id]];
                throw new PluginDependencyException(
                    $"Two plugins share the id '{lPlugin.Id}': '{lOther.GetType().FullName}' and '{lPlugin.GetType().FullName}'.");
            }
        }

        // lDependencies[i] = indices of the plugins that plugin i must follow.
        List<SortedSet<int>> lDependencies = new List<SortedSet<int>>(pPlugins.Count);
        List<string> lErrors = new List<string>();

        for (int lIndex = 0; lIndex < pPlugins.Count; lIndex++)
        {
            IPlugin lPlugin = pPlugins[lIndex];
            Type lType = lPlugin.GetType();
            SortedSet<int> lDeps = new SortedSet<int>();

            foreach (DependsOnPluginAttribute lAttr in lType.GetCustomAttributes<DependsOnPluginAttribute>())
            {
                if (lIndexById.TryGetValue(lAttr.PluginId, out int lDepIndex))
                {
                    if (lDepIndex != lIndex)
                    {
                        lDeps.Add(lDepIndex);
                    }
                }
                else if (!lAttr.Optional)
                {
                    lErrors.Add($"Plugin '{lPlugin.Id}' depends on plugin '{lAttr.PluginId}', which is not installed.");
                }
            }

            foreach (DependsOnServiceAttribute lAttr in lType.GetCustomAttributes<DependsOnServiceAttribute>())
            {
                for (int lProviderIndex = 0; lProviderIndex < pPlugins.Count; lProviderIndex++)
                {
                    if (lProviderIndex != lIndex && pPlugins[lProviderIndex].ProvidedServices.Contains(lAttr.ServiceType))
                    {
                        lDeps.Add(lProviderIndex);
                    }
                }
            }

            lDependencies.Add(lDeps);
        }

        if (lErrors.Count > 0)
        {
            throw new PluginDependencyException(
                "Missing plugin dependencies:" + Environment.NewLine + string.Join(Environment.NewLine, lErrors));
        }

        // Kahn's algorithm, always picking the ready plugin with the lowest input index.
        int[] lPendingCount = lDependencies.Select(pDeps => pDeps.Count).ToArray();
        List<int>[] lDependents = Enumerable.Range(0, pPlugins.Count).Select(_ => new List<int>()).ToArray();
        for (int lIndex = 0; lIndex < pPlugins.Count; lIndex++)
        {
            foreach (int lDepIndex in lDependencies[lIndex])
            {
                lDependents[lDepIndex].Add(lIndex);
            }
        }

        SortedSet<int> lReady = new SortedSet<int>(Enumerable.Range(0, pPlugins.Count).Where(pI => lPendingCount[pI] == 0));
        List<IPlugin> lSorted = new List<IPlugin>(pPlugins.Count);

        while (lReady.Count > 0)
        {
            int lNext = lReady.Min;
            lReady.Remove(lNext);
            lSorted.Add(pPlugins[lNext]);

            foreach (int lDependent in lDependents[lNext])
            {
                if (--lPendingCount[lDependent] == 0)
                {
                    lReady.Add(lDependent);
                }
            }
        }

        if (lSorted.Count < pPlugins.Count)
        {
            List<int> lCycle = FindCycle(lDependencies, lPendingCount);
            string lPath = string.Join(" -> ", lCycle.Select(pI => $"'{pPlugins[pI].Id}'"));
            throw new PluginDependencyException($"Plugin dependency cycle detected: {lPath}.");
        }

        return lSorted;
    }

    // Walks dependencies among the unsorted plugins until one repeats; every unsorted plugin
    // still has an unsorted dependency, so the walk always closes a cycle.
    private static List<int> FindCycle(List<SortedSet<int>> pDependencies, int[] pPendingCount)
    {
        int lCurrent = Array.FindIndex(pPendingCount, pCount => pCount > 0);
        List<int> lPath = new List<int>();
        Dictionary<int, int> lPositionInPath = new Dictionary<int, int>();

        while (!lPositionInPath.ContainsKey(lCurrent))
        {
            lPositionInPath[lCurrent] = lPath.Count;
            lPath.Add(lCurrent);
            lCurrent = pDependencies[lCurrent].First(pDep => pPendingCount[pDep] > 0);
        }

        List<int> lCycle = lPath.GetRange(lPositionInPath[lCurrent], lPath.Count - lPositionInPath[lCurrent]);
        lCycle.Add(lCurrent);
        return lCycle;
    }
}
