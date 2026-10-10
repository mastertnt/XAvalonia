using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Shell.Abstractions.Logging;
using XAvalonia.Shell.Abstractions.Plugins;
using XAvalonia.Shell.Abstractions.Shell;
using XAvalonia.Shell.Abstractions.Splashscreen;

namespace XAvalonia.Splashscreen;

/// <summary>
/// Plugin that registers <see cref="ISplashscreen"/> and shows the splash screen during startup.
/// The image and appearance come from the <c>"Splashscreen"</c> section of <c>technical_configuration.json</c>.
/// The splash screen closes automatically when the main window is loaded.
/// </summary>
public sealed class SplashscreenPlugin : IPlugin
{
    private readonly SplashscreenService mService = new SplashscreenService();

    /// <inheritdoc/>
    public string Id => "org.avaloniaui.shell.splashscreen";

    /// <inheritdoc/>
    public string Name => "Splashscreen";

    /// <inheritdoc/>
    public string Description => "Displays a configurable splash screen with status messages pushed by plugins during startup.";

    /// <inheritdoc/>
    public Version CurrentVersion => new Version(1, 0, 0);

    /// <inheritdoc/>
    public string? NuGetPackageId => null;

    /// <inheritdoc/>
    public IReadOnlyList<Type> ProvidedServices => new[] { typeof(ISplashscreen) };

    /// <summary>When <c>false</c>, the service is still registered but the window is never shown.</summary>
    [TechConfiguration("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>Image to display: <c>avares://</c> URI, absolute path, or path relative to the application directory.</summary>
    [TechConfiguration("imagePath")]
    public string? ImagePath { get; set; }

    /// <summary>Window width in pixels; defaults to the image width.</summary>
    [TechConfiguration("width")]
    public int? Width { get; set; }

    /// <summary>Window height in pixels; defaults to the image height.</summary>
    [TechConfiguration("height")]
    public int? Height { get; set; }

    /// <summary>Window background color.</summary>
    [TechConfiguration("background")]
    public string Background { get; set; } = "#1E1E1E";

    /// <summary>Message text color.</summary>
    [TechConfiguration("foreground")]
    public string Foreground { get; set; } = "#FFFFFF";

    /// <summary>Minimum time, in milliseconds, the splash screen stays visible.</summary>
    [TechConfiguration("minimumDisplayTime")]
    public int MinimumDisplayTime { get; set; }

    /// <inheritdoc/>
    public void RegisterServices(IServiceCollection pServices)
    {
        pServices.AddSingleton<ISplashscreen>(mService);
    }

    /// <inheritdoc/>
    public void Initialize(IPluginServiceManager pServiceManager)
    {
        ILogService? lLog = pServiceManager.TryRequestService<ILogService>();
        lLog?.LogInfo($"[{Name}] Loading plugin…");

        if (!Enabled)
        {
            mService.Close();
            lLog?.LogInfo($"[{Name}] Disabled by configuration.");
            return;
        }

        mService.Configure(new SplashscreenOptions
        {
            ImagePath          = ImagePath,
            Width              = Width,
            Height             = Height,
            Background         = Background,
            Foreground         = Foreground,
            MinimumDisplayTime = TimeSpan.FromMilliseconds(Math.Max(0, MinimumDisplayTime))
        });

        lLog?.LogDebug($"[{Name}] Requesting IMainWindowService…");
        IMainWindowService? lMainWindow = pServiceManager.TryRequestService<IMainWindowService>();
        if (lMainWindow is not null)
        {
            lMainWindow.Loaded += OnMainWindowLoaded;
            lLog?.LogDebug($"[{Name}] Will close when the main window is loaded.");
        }
        else
        {
            lLog?.LogWarning($"[{Name}] IMainWindowService not available; call ISplashscreen.Close() to hide the splash screen.");
        }

        mService.Show();
        lLog?.LogInfo($"[{Name}] Plugin loaded successfully.");
    }

    private void OnMainWindowLoaded(object? pSender, EventArgs pArgs)
    {
        if (pSender is IMainWindowService lMainWindow)
        {
            lMainWindow.Loaded -= OnMainWindowLoaded;
        }

        mService.CloseAfterMinimumDisplayTime();
    }
}
