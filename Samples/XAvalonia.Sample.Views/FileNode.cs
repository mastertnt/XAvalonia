using Avalonia.Media;

namespace XAvalonia.Sample.Views;

/// <summary>Represents a file or folder node in the explorer tree.</summary>
public sealed class FileNode
{
    /// <summary>Initializes a new file or folder node.</summary>
    /// <param name="pName">Display name shown in the tree.</param>
    /// <param name="pIcon">Icon resolved from <c>IIconManager</c>, or <c>null</c> if unavailable.</param>
    /// <param name="pChildren">Child nodes for folders; omit or pass <c>null</c> for files.</param>
    public FileNode(string pName, IImage? pIcon, IReadOnlyList<FileNode>? pChildren = null)
    {
        Name     = pName;
        Icon     = pIcon;
        Children = pChildren ?? Array.Empty<FileNode>();
    }

    /// <summary>Display name of the file or folder.</summary>
    public string Name { get; }

    /// <summary>Icon resolved from <c>IIconManager</c>.</summary>
    public IImage? Icon { get; }

    /// <summary>Child nodes; empty for files.</summary>
    public IReadOnlyList<FileNode> Children { get; }
}
