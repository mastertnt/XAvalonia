using Avalonia.Media;
using IconPacks.Avalonia.Material;
using XAvalonia.Shell.Abstractions.Icons;

namespace XAvalonia.IconProviders;

/// <summary>
/// <see cref="IIconProvider"/> backed by <c>IconPacks.Avalonia.Material</c>.
/// Converts icon geometry to <see cref="DrawingImage"/> instances.
/// </summary>
public sealed class MaterialIconProvider : IIconProvider
{
    private static readonly IReadOnlyDictionary<string, PackIconMaterialKind> IconMap =
        new Dictionary<string, PackIconMaterialKind>(StringComparer.OrdinalIgnoreCase)
        {
            ["Folder"] = PackIconMaterialKind.Folder,
            ["File"]   = PackIconMaterialKind.FileDocument,
        };

    /// <inheritdoc/>
    public bool Contains(string pIconName) => IconMap.ContainsKey(pIconName);

    /// <inheritdoc/>
    public IImage? GetIcon(string pIconName, double pSize, IBrush pForeground, double pRenderScaling = 1.0)
    {
        if (!IconMap.TryGetValue(pIconName, out PackIconMaterialKind lKind))
        {
            return null;
        }

        PackIconMaterial lControl = new PackIconMaterial { Kind = lKind };
        Geometry? lGeometry = lControl.Data;

        if (lGeometry is null)
        {
            return null;
        }

        GeometryDrawing lDrawing = new GeometryDrawing
        {
            Geometry = lGeometry,
            Brush    = pForeground,
        };

        return new DrawingImage(lDrawing);
    }
}
