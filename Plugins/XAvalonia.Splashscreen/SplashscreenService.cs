using Avalonia.Threading;
using XAvalonia.Shell.Abstractions.Splashscreen;

namespace XAvalonia.Splashscreen;

/// <summary>
/// Default implementation of <see cref="ISplashscreen"/>.
/// Calls from a background thread are posted to the UI thread; <see cref="MessageChanged"/> is raised on the calling thread.
/// </summary>
public sealed class SplashscreenService : ISplashscreen
{
    private SplashscreenOptions mOptions = new SplashscreenOptions();
    private SplashscreenWindow? mWindow;
    private DateTime mShownAt;
    private bool mClosed;
    private volatile string? mCurrentMessage;

    /// <inheritdoc/>
    public event EventHandler<string>? MessageChanged;

    /// <inheritdoc/>
    public bool IsVisible => mWindow is not null;

    /// <inheritdoc/>
    public string? CurrentMessage => mCurrentMessage;

    /// <inheritdoc/>
    public void Show()
    {
        RunOnUiThread(ShowCore);
    }

    /// <inheritdoc/>
    public void ShowMessage(string pMessage)
    {
        mCurrentMessage = pMessage;
        MessageChanged?.Invoke(this, pMessage);

        RunOnUiThread(() =>
        {
            if (mWindow is null)
            {
                return;
            }

            mWindow.SetMessage(pMessage);
            Refresh();
        });
    }

    /// <inheritdoc/>
    public void Close()
    {
        RunOnUiThread(CloseCore);
    }

    /// <summary>Applies the options read from the technical configuration. Must be called before <see cref="Show"/>.</summary>
    internal void Configure(SplashscreenOptions pOptions)
    {
        mOptions = pOptions;
    }

    /// <summary>Closes the splash screen once it has been visible for at least the configured minimum time.</summary>
    internal void CloseAfterMinimumDisplayTime()
    {
        RunOnUiThread(() =>
        {
            TimeSpan lRemaining = mOptions.MinimumDisplayTime - (DateTime.UtcNow - mShownAt);
            if (mWindow is null || lRemaining <= TimeSpan.Zero)
            {
                CloseCore();
                return;
            }

            DispatcherTimer.RunOnce(CloseCore, lRemaining);
        });
    }

    private void ShowCore()
    {
        if (mWindow is not null || mClosed)
        {
            return;
        }

        mWindow = new SplashscreenWindow(mOptions);
        if (mCurrentMessage is not null)
        {
            mWindow.SetMessage(mCurrentMessage);
        }

        mWindow.Show();
        mShownAt = DateTime.UtcNow;
        Refresh();
    }

    private void CloseCore()
    {
        if (mClosed)
        {
            return;
        }

        mClosed = true;
        mWindow?.Close();
        mWindow = null;
    }

    // Plugins are initialized synchronously on the UI thread, so the dispatcher would not render
    // the splash screen before startup ends. Running the pending jobs lets the window repaint now.
    private static void Refresh()
    {
        Dispatcher.UIThread.RunJobs(null);
    }

    private static void RunOnUiThread(Action pAction)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            pAction();
        }
        else
        {
            Dispatcher.UIThread.Post(pAction);
        }
    }
}
