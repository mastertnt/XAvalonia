using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;
using XAvalonia.Shell.Abstractions.Logging;

namespace XAvalonia.LogViewer;

/// <summary>
/// View model for the Log Viewer tool panel.
/// Subscribes to <see cref="ILogService"/> events and exposes a filtered list of entries.
/// </summary>
public sealed class LogViewerViewModel : Tool
{
    private readonly ILogService mLogService;
    private readonly List<LogEntryViewModel> mAllEntries = new List<LogEntryViewModel>();
    private bool mShowDebug   = true;
    private bool mShowInfo    = true;
    private bool mShowWarning = true;
    private bool mShowError   = true;

    /// <summary>Initializes the view model and subscribes to the log service.</summary>
    /// <param name="pLogService">The log service to observe.</param>
    public LogViewerViewModel(ILogService pLogService)
    {
        Id    = "log-viewer";
        Title = "Log";
        DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanPin   = true,
            CanFloat = true,
            CanClose = true
        };

        mLogService = pLogService;
        mLogService.MessageLogged += OnMessageLogged;
        mLogService.Cleared       += OnCleared;

        ClearCommand = new RelayCommand(mLogService.Clear);
    }

    /// <summary>Filtered entries currently visible in the view.</summary>
    public ObservableCollection<LogEntryViewModel> FilteredEntries { get; } = new ObservableCollection<LogEntryViewModel>();

    /// <summary>Clears all entries from the log.</summary>
    public ICommand ClearCommand { get; }

    /// <summary>Whether Debug-level entries are shown.</summary>
    public bool ShowDebug
    {
        get => mShowDebug;
        set
        {
            this.RaiseAndSetIfChanged(ref mShowDebug, value);
            RefreshFilter();
        }
    }

    /// <summary>Whether Info-level entries are shown.</summary>
    public bool ShowInfo
    {
        get => mShowInfo;
        set
        {
            this.RaiseAndSetIfChanged(ref mShowInfo, value);
            RefreshFilter();
        }
    }

    /// <summary>Whether Warning-level entries are shown.</summary>
    public bool ShowWarning
    {
        get => mShowWarning;
        set
        {
            this.RaiseAndSetIfChanged(ref mShowWarning, value);
            RefreshFilter();
        }
    }

    /// <summary>Whether Error-level entries are shown.</summary>
    public bool ShowError
    {
        get => mShowError;
        set
        {
            this.RaiseAndSetIfChanged(ref mShowError, value);
            RefreshFilter();
        }
    }

    private void OnMessageLogged(object? pSender, LogMessageEventArgs pArgs)
    {
        LogEntryViewModel lEntry = new LogEntryViewModel(pArgs.Level, pArgs.Message);
        Dispatcher.UIThread.Post(() =>
        {
            mAllEntries.Add(lEntry);
            if (IsLevelVisible(lEntry.Level))
            {
                FilteredEntries.Add(lEntry);
            }
        });
    }

    private void OnCleared(object? pSender, EventArgs pArgs)
    {
        Dispatcher.UIThread.Post(() =>
        {
            mAllEntries.Clear();
            FilteredEntries.Clear();
        });
    }

    private void RefreshFilter()
    {
        FilteredEntries.Clear();
        foreach (LogEntryViewModel lEntry in mAllEntries)
        {
            if (IsLevelVisible(lEntry.Level))
            {
                FilteredEntries.Add(lEntry);
            }
        }
    }

    private bool IsLevelVisible(LogLevel pLevel)
    {
        return pLevel switch
        {
            LogLevel.Debug   => mShowDebug,
            LogLevel.Info    => mShowInfo,
            LogLevel.Warning => mShowWarning,
            LogLevel.Error   => mShowError,
            _                => true,
        };
    }
}
