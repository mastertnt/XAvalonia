using Avalonia.Media;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using XAvalonia.Shell.Abstractions.Icons;

namespace XAvalonia.Sample.Views;

/// <summary>View model for the Explorer tool panel (left side).</summary>
public sealed class ExplorerViewModel : Tool
{
    private static readonly IBrush FolderBrush = new SolidColorBrush(Color.Parse("#5B9BD5"));
    private static readonly IBrush FileBrush   = new SolidColorBrush(Color.Parse("#808080"));

    /// <summary>Initializes the explorer panel and resolves icons via <paramref name="pIconManager"/>.</summary>
    public ExplorerViewModel(IIconManager pIconManager)
    {
        Id    = "explorer";
        Title = "Explorateur";
        DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanPin   = true,
            CanFloat = true,
            CanClose = true
        };

        IImage? lFolderIcon = pIconManager.GetIcon("Folder", 16, FolderBrush);
        IImage? lFileIcon   = pIconManager.GetIcon("File",   16, FileBrush);

        Nodes = BuildSampleTree(lFolderIcon, lFileIcon);
    }

    /// <summary>Root nodes of the explorer tree.</summary>
    public IReadOnlyList<FileNode> Nodes { get; }

    private static IReadOnlyList<FileNode> BuildSampleTree(IImage? pFolderIcon, IImage? pFileIcon)
    {
        return new FileNode[]
        {
            new FileNode("src", pFolderIcon, new FileNode[]
            {
                new FileNode("Program.cs",  pFileIcon),
                new FileNode("App.axaml",   pFileIcon),
                new FileNode("Views", pFolderIcon, new FileNode[]
                {
                    new FileNode("MainWindow.axaml", pFileIcon),
                }),
                new FileNode("ViewModels", pFolderIcon, new FileNode[]
                {
                    new FileNode("MainWindowViewModel.cs", pFileIcon),
                }),
            }),
            new FileNode("Assets", pFolderIcon),
        };
    }
}
