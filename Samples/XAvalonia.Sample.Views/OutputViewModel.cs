using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;

namespace XAvalonia.Sample.Views;

/// <summary>View model for the Output tool panel (bottom).</summary>
public sealed class OutputViewModel : Tool
{
    private string mLog = "[INFO] Application démarrée.\n[INFO] Dock layout chargé.\n";

    /// <summary>Initializes the output panel.</summary>
    public OutputViewModel()
    {
        Id    = "output";
        Title = "Sortie";
        DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanPin   = true,
            CanFloat = true,
            CanClose = true
        };
    }

    /// <summary>Content of the output log, bound to the view's TextBlock.</summary>
    public string Log
    {
        get => mLog;
        set => this.RaiseAndSetIfChanged(ref mLog, value);
    }

    /// <summary>Appends a line to the output log.</summary>
    /// <param name="pLine">Text to append.</param>
    public void AppendLine(string pLine)
    {
        Log += $"{pLine}\n";
    }
}
