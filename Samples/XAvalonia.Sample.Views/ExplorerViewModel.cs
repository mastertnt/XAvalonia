using Avalonia.Media;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using ReactiveUI;
using XAvalonia.Shell.Abstractions.Icons;
using XAvalonia.Shell.Abstractions.Selection;

namespace XAvalonia.Sample.Views;

/// <summary>View model for the Explorer tool panel (left side).</summary>
public sealed class ExplorerViewModel : Tool
{
    private static readonly IBrush FolderBrush = new SolidColorBrush(Color.Parse("#5B9BD5"));
    private static readonly IBrush FileBrush   = new SolidColorBrush(Color.Parse("#808080"));

    private readonly ISelectionManager mSelectionManager;
    private FileNode? mSelectedNode;

    /// <summary>Initializes the explorer panel and resolves icons via <paramref name="pIconManager"/>.</summary>
    /// <param name="pIconManager">Resolves the folder and file icons.</param>
    /// <param name="pSelectionManager">Receives the selected node in its global context.</param>
    public ExplorerViewModel(IIconManager pIconManager, ISelectionManager pSelectionManager)
    {
        mSelectionManager = pSelectionManager;
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

    /// <summary>Node selected in the tree, published to the global selection context.</summary>
    public FileNode? SelectedNode
    {
        get => mSelectedNode;
        set
        {
            if (ReferenceEquals(mSelectedNode, value))
            {
                return;
            }

            this.RaiseAndSetIfChanged(ref mSelectedNode, value);

            if (value is null)
            {
                mSelectionManager.GlobalContext.Clear();
            }
            else
            {
                mSelectionManager.GlobalContext.Select(value);
            }
        }
    }

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
