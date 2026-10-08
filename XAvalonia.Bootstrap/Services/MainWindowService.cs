using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Bootstrap.Converters;
using XAvalonia.Bootstrap.ViewModels;
using XAvalonia.Shell.Abstractions.Shell;

namespace XAvalonia.Bootstrap.Services;

/// <summary>
/// <see cref="IMainWindowService"/> implementation.
/// Updates the <see cref="MainWindowViewModel"/> for title and icon changes
/// (the view binds to those properties), and delegates shutdown to the
/// Avalonia application lifetime.
/// Layout save/load are delegated to the optional <see cref="ILayoutPersistence"/>
/// registered by the active layout plugin.
/// Registered as singleton under both <c>MainWindowService</c> and <c>IMainWindowService</c>.
/// </summary>
public sealed class MainWindowService : IMainWindowService
{
    private readonly MainWindowViewModel mViewModel;
    private readonly IServiceProvider mServiceProvider;

    /// <summary>Initializes the service with the shell's main window view model.</summary>
    /// <param name="pViewModel">The singleton main window view model.</param>
    /// <param name="pServiceProvider">Used to lazily resolve the optional <see cref="ILayoutPersistence"/>.</param>
    public MainWindowService(MainWindowViewModel pViewModel, IServiceProvider pServiceProvider)
    {
        mViewModel       = pViewModel;
        mServiceProvider = pServiceProvider;
    }

    /// <inheritdoc/>
    public event EventHandler? Loaded;

    /// <inheritdoc/>
    public event EventHandler? AboutToQuit;

    /// <inheritdoc/>
    public event EventHandler? Closed;

    /// <summary>
    /// Subscribes to the given window's lifecycle events so that <see cref="Loaded"/>,
    /// <see cref="AboutToQuit"/> and <see cref="Closed"/> are raised at the right moments.
    /// Must be called once, right after the main window is created.
    /// </summary>
    /// <param name="pWindow">The shell's main window.</param>
    public void AttachWindow(Window pWindow)
    {
        pWindow.Opened  += OnWindowOpened;
        pWindow.Closing += OnWindowClosing;
        pWindow.Closed  += OnWindowClosed;
    }

    /// <inheritdoc/>
    public void SetTitle(string pTitle)
    {
        mViewModel.WindowTitle = pTitle;
    }

    /// <inheritdoc/>
    public void SetIcon(string? pIconUri)
    {
        Bitmap? lBitmap = IconLoader.Load(pIconUri);
        mViewModel.WindowIcon = lBitmap is null ? null : new WindowIcon(lBitmap);
    }

    /// <inheritdoc/>
    public void Shutdown()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lLifetime)
        {
            lLifetime.Shutdown();
        }
    }

    private void OnWindowOpened(object? pSender, EventArgs pArgs)
    {
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    private void OnWindowClosing(object? pSender, WindowClosingEventArgs pArgs)
    {
        AboutToQuit?.Invoke(this, EventArgs.Empty);
    }

    private void OnWindowClosed(object? pSender, EventArgs pArgs)
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public void SaveLayout(string pFilePath)
    {
        mServiceProvider.GetService<ILayoutPersistence>()?.SaveLayout(pFilePath);
    }

    /// <inheritdoc/>
    public void LoadLayout(string pFilePath)
    {
        mServiceProvider.GetService<ILayoutPersistence>()?.LoadLayout(pFilePath);
    }
}
