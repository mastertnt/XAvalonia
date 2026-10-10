using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.MainWindow.Docking;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Controls;
using Dock.Serializer;
using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Shell.Abstractions.Documents;
using XAvalonia.Shell.Abstractions.Logging;
using XAvalonia.Shell.Abstractions.Menus;
using XAvalonia.Shell.Abstractions.Plugins;
using XAvalonia.Shell.Abstractions.Shell;
using XAvalonia.Shell.Abstractions.StatusBar;
using XAvalonia.Shell.Abstractions.ToolPanels;

namespace Avalonia.MainWindow;

/// <summary>
/// Plugin that contributes the main docking layout (Explorer, Documents, Properties, Output)
/// together with the File, View and Help menus and the default status bar items.
/// Also implements <see cref="ILayoutPersistence"/> to save and restore the dock layout.
/// </summary>
[DependsOnService(typeof(ILogService))]
[ConfigurationSection("Avalonia.MainWindow")]
public sealed class AvaloniaMainWindowPlugin : IPlugin, ILayoutPersistence, IMainWindowService
{
    // These events satisfy IMainWindowService; this plugin's DI registration is overridden
    // by the bootstrap's MainWindowService, so these are never actually raised at runtime.
#pragma warning disable CS0067
    public event EventHandler? Loaded;
    public event EventHandler? AboutToQuit;
    public event EventHandler? Closed;
#pragma warning restore CS0067

    /// <summary>Application name displayed in the window title. Populated from <c>technical_configuration.json → AvaloniaMainWindow → name</c>.</summary>
    [TechConfiguration("name")]
    public string ApplicationName { get; set; } = "XAvalonia";

    /// <summary>Application icon URI. Supports <c>avares://</c> and file paths. Populated from <c>technical_configuration.json → AvaloniaMainWindow → iconUri</c>.</summary>
    [TechConfiguration("iconUri")]
    public string? IconUri { get; set; }

    private IStatusBarService? mStatusBarService;
    private IShellDockService? mDockService;
    private IDocumentDockConnector? mDocumentDockConnector;
    private IToolPanelDockConnector? mToolPanelDockConnector;
    private ShellDockFactory? mFactory;
    private DockSerializer? mSerializer;
    private IRootDock? mLayout;


    /// <inheritdoc/>
    public string Id => "org.avaloniaui.shell.mainwindow";

    /// <inheritdoc/>
    public string Name => "Main Window";

    /// <inheritdoc/>
    public string Description => "Dock layout with Explorer, Documents, Properties and Output panels.";

    /// <inheritdoc/>
    public Version CurrentVersion => new Version(1, 0, 0);

    /// <inheritdoc/>
    public string? NuGetPackageId => null;

    /// <inheritdoc/>
    public IReadOnlyList<Type> ProvidedServices => new[] { typeof(ILayoutPersistence), typeof(IMainWindowService) };

    /// <inheritdoc/>
    public void RegisterServices(IServiceCollection pServices)
    {
        pServices.AddSingleton<ILayoutPersistence>(this);
    }

    /// <inheritdoc/>
    public void SetTitle(string pTitle)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lLifetime
            && lLifetime.MainWindow is not null)
        {
            lLifetime.MainWindow.Title = pTitle;
        }
    }

    /// <inheritdoc/>
    public void SetIcon(string? pIconUri)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime lLifetime
            || lLifetime.MainWindow is null)
        {
            return;
        }

        if (pIconUri is null)
        {
            lLifetime.MainWindow.Icon = null;
            return;
        }

        Bitmap? lBitmap;
        if (pIconUri.StartsWith("avares://", StringComparison.Ordinal))
        {
            lBitmap = new Bitmap(AssetLoader.Open(new Uri(pIconUri)));
        }
        else
        {
            lBitmap = new Bitmap(pIconUri);
        }

        lLifetime.MainWindow.Icon = new WindowIcon(lBitmap);
    }

    /// <inheritdoc/>
    public void Shutdown()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lLifetime)
        {
            lLifetime.Shutdown();
        }
    }

    /// <inheritdoc/>
    public void Initialize(IPluginServiceManager pServiceManager)
    {
        ILogService? lLog = pServiceManager.TryRequestService<ILogService>();
        lLog?.LogInfo($"[{Name}] Loading plugin…");

        lLog?.LogDebug($"[{Name}] Requesting IShellDockService…");
        mDockService = pServiceManager.RequestService<IShellDockService>();
        lLog?.LogDebug($"[{Name}] IShellDockService acquired.");

        lLog?.LogDebug($"[{Name}] Requesting IStatusBarService…");
        mStatusBarService = pServiceManager.RequestService<IStatusBarService>();
        lLog?.LogDebug($"[{Name}] IStatusBarService acquired.");

        lLog?.LogDebug($"[{Name}] Requesting IDocumentDockConnector…");
        mDocumentDockConnector = pServiceManager.RequestService<IDocumentDockConnector>();
        lLog?.LogDebug($"[{Name}] IDocumentDockConnector acquired.");

        lLog?.LogDebug($"[{Name}] Requesting IToolPanelDockConnector…");
        mToolPanelDockConnector = pServiceManager.RequestService<IToolPanelDockConnector>();
        lLog?.LogDebug($"[{Name}] IToolPanelDockConnector acquired.");

        lLog?.LogDebug($"[{Name}] Requesting IMenuService…");
        IMenuService lMenuService = pServiceManager.RequestService<IMenuService>();
        lLog?.LogDebug($"[{Name}] IMenuService acquired.");

        lLog?.LogDebug($"[{Name}] Requesting IMainWindowService…");
        IMainWindowService lWindowService = pServiceManager.RequestService<IMainWindowService>();
        lLog?.LogDebug($"[{Name}] IMainWindowService acquired.");

        SetupDockLayout();
        lLog?.LogDebug($"[{Name}] Dock layout initialized.");

        RegisterMenus(lMenuService);
        lLog?.LogDebug($"[{Name}] Menus registered.");

        RegisterStatusBarItems();
        lLog?.LogDebug($"[{Name}] Status bar items registered.");

        lLog?.LogInfo($"[{Name}] Plugin loaded successfully.");

        IPluginManager lPluginManager = pServiceManager.RequestService<IPluginManager>();
        lPluginManager.AllPluginsLoaded += (pSender, pArgs) =>
        {
            lWindowService.SetTitle(ApplicationName);
            lWindowService.SetIcon(IconUri);
            LoadLayout(IMainWindowService.DefaultLayoutPath);
        };
    }

    /// <inheritdoc/>
    public void SaveLayout(string pFilePath)
    {
        if (mSerializer is null || mLayout is null)
        {
            return;
        }

        try
        {
            using FileStream lStream = File.Create(pFilePath);
            mSerializer.Save(lStream, mLayout);
        }
        catch (Exception lEx)
        {
            _ = lEx;
        }
    }

    /// <inheritdoc/>
    public void LoadLayout(string pFilePath)
    {
        if (mFactory is null || mSerializer is null || !File.Exists(pFilePath))
        {
            return;
        }

        try
        {
            using FileStream lStream = File.OpenRead(pFilePath);
            IRootDock? lLoaded = mSerializer.Load<IRootDock>(lStream);

            if (lLoaded is null)
            {
                return;
            }

            mFactory.InitLayout(lLoaded);
            mDockService!.SetLayout(lLoaded);
            mDocumentDockConnector!.ConnectDock(mFactory, mFactory.DocumentDock!);
            mToolPanelDockConnector!.ConnectDocks(mFactory, mFactory.LeftDock, mFactory.RightDock, mFactory.BottomDock);
            mLayout = lLoaded;
        }
        catch (Exception lEx)
        {
            _ = lEx;
        }
    }

    // Builds the dock layout, injects it into the shell, and connects document + panel services.
    private void SetupDockLayout()
    {
        mFactory    = new ShellDockFactory();
        mSerializer = new DockSerializer(typeof(ObservableCollection<>));

        mLayout = mFactory.CreateLayout();
        mFactory.InitLayout(mLayout);
        mDockService!.SetLayout(mLayout);
        mDocumentDockConnector!.ConnectDock(mFactory, mFactory.DocumentDock!);
        mToolPanelDockConnector!.ConnectDocks(mFactory, mFactory.LeftDock, mFactory.RightDock, mFactory.BottomDock);
     }

    private void RegisterMenus(IMenuService pMenuService)
    {
        pMenuService.RegisterMenu(new MenuContribution("file", "_File", 100));
        pMenuService.RegisterMenuItem("file", new MenuItemContribution("file.new",  "_New",   new RelayCommand(NewFile),   100, "Ctrl+N"));
        pMenuService.RegisterMenuItem("file", new MenuItemContribution("file.open", "_Open…", new RelayCommand(OpenFile),  200, "Ctrl+O"));
        pMenuService.RegisterMenuItem("file", new MenuItemContribution("file.save", "_Save",  new RelayCommand(SaveFile),  300, "Ctrl+S"));
        pMenuService.RegisterMenuItem("file", new MenuSeparatorContribution("file.sep1", 400));
        pMenuService.RegisterMenuItem("file", new MenuItemContribution("file.exit", "E_xit",  new RelayCommand(Exit),      500));

        pMenuService.RegisterMenu(new MenuContribution("view", "_View", 200));
        pMenuService.RegisterMenuItem("view", new MenuItemContribution("view.resetLayout", "Reset Layout", new RelayCommand(ResetLayout), 100));

        pMenuService.RegisterMenu(new MenuContribution("help", "_Help", 900));
        pMenuService.RegisterMenuItem("help", new MenuItemContribution("help.about", "_About…", new RelayCommand(About), 100));
    }

    private void RegisterStatusBarItems()
    {
        mStatusBarService!.RegisterItem(new StatusBarItemContribution("status.message", "Ready",        StatusBarItemAlignment.Left,  100));
        mStatusBarService.RegisterItem(new StatusBarItemContribution("status.file",    "No file open",  StatusBarItemAlignment.Left,  200));
        mStatusBarService.RegisterItem(new StatusBarItemContribution("status.line",    "Ln 1",          StatusBarItemAlignment.Right, 100));
        mStatusBarService.RegisterItem(new StatusBarItemContribution("status.col",     "Col 1",         StatusBarItemAlignment.Right, 200));
    }

    private void NewFile()
    {
        mStatusBarService!.UpdateItem("status.message", "New file created");
        mStatusBarService.UpdateItem("status.file", "Untitled");
    }

    private void OpenFile()
    {
        mStatusBarService!.UpdateItem("status.message", "Opening a file…");
    }

    private void SaveFile()
    {
        mStatusBarService!.UpdateItem("status.message", "File saved");
    }

    private void Exit()
    {
        Shutdown();
    }

    private void About()
    {
        mStatusBarService!.UpdateItem("status.message", "AvaloniaShell — Dock + MenuBar + StatusBar + DI");
    }

    private void ResetLayout()
    {
        if (mFactory is null || mDockService is null)
        {
            return;
        }

        mLayout = mFactory.CreateLayout();
        mFactory.InitLayout(mLayout);
        mDockService.SetLayout(mLayout);
        mDocumentDockConnector!.ConnectDock(mFactory, mFactory.DocumentDock!);
        mToolPanelDockConnector!.ConnectDocks(mFactory, mFactory.LeftDock, mFactory.RightDock, mFactory.BottomDock);
        mStatusBarService!.UpdateItem("status.message", "Layout reset");
    }


}
