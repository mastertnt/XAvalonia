using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;
using XAvalonia.Shell.Abstractions.Plugins;

namespace PluginBrowser.ViewModels;

/// <summary>
/// View model for the Plugin Browser document tab.
/// Populates the plugin list on construction and fetches available NuGet versions asynchronously.
/// </summary>
public sealed class PluginBrowserViewModel : Document
{
    private readonly IPluginManager mPluginManager;
    private bool mIsLoadingVersions;
    private PluginViewModel? mSelectedPlugin;

    /// <summary>Initializes the view model and builds the initial plugin list.</summary>
    /// <param name="pPluginManager">The plugin manager to read plugins from.</param>
    public PluginBrowserViewModel(IPluginManager pPluginManager)
    {
        Id    = "plugin-browser";
        Title = "Plugin Browser";
        DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanPin = true,
            CanFloat = true,
            CanClose = true
        };

        mPluginManager = pPluginManager;
        Plugins = pPluginManager.Plugins
            .Select(pP => new PluginViewModel(pP, pPluginManager.GetConsumedServices(pP.Id)))
            .ToList();
    }

    /// <summary>All loaded plugins, one view model per plugin.</summary>
    public IReadOnlyList<PluginViewModel> Plugins { get; }

    /// <summary>Whether NuGet versions are currently being fetched.</summary>
    public bool IsLoadingVersions
    {
        get => mIsLoadingVersions;
        private set => this.RaiseAndSetIfChanged(ref mIsLoadingVersions, value);
    }

    /// <summary>Currently selected plugin node in the tree.</summary>
    public PluginViewModel? SelectedPlugin
    {
        get => mSelectedPlugin;
        set => this.RaiseAndSetIfChanged(ref mSelectedPlugin, value);
    }

    /// <summary>
    /// Fetches the latest NuGet version for each plugin that declares a <c>NuGetPackageId</c>.
    /// Updates <see cref="PluginViewModel.AvailableVersion"/> as results arrive.
    /// </summary>
    /// <param name="pCancellationToken">Token used to cancel the operation.</param>
    public async Task LoadAvailableVersionsAsync(CancellationToken pCancellationToken = default)
    {
        IsLoadingVersions = true;

        try
        {
            foreach (PluginViewModel lVm in Plugins)
            {
                if (pCancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (lVm.NuGetPackageId is null)
                {
                    lVm.AvailableVersion = "N/A";
                    continue;
                }

                System.Version? lVersion = await mPluginManager
                    .GetAvailableVersionAsync(lVm.NuGetPackageId, pCancellationToken)
                    .ConfigureAwait(false);

                lVm.AvailableVersion = lVersion?.ToString() ?? "unavailable";
            }
        }
        finally
        {
            IsLoadingVersions = false;
        }
    }
}
