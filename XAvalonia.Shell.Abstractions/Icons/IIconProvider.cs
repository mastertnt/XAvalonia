using Avalonia.Media;

namespace XAvalonia.Shell.Abstractions.Icons;

/// <summary>
/// Fournit des icônes Avalonia à une taille et une couleur de premier plan données.
/// </summary>
public interface IIconProvider
{
    /// <summary>
    /// Indique si l'icône identifiée par <paramref name="pIconName"/> est disponible dans ce fournisseur.
    /// </summary>
    bool Contains(string pIconName);

    /// <summary>
    /// Retourne l'icône demandée, ou null si elle est introuvable.
    /// </summary>
    /// <param name="pIconName">Identifiant logique de l'icône (ex. "Save", "Folder").</param>
    /// <param name="pSize">Taille en unités indépendantes de l'appareil (icône carrée).</param>
    /// <param name="pForeground">Brush appliqué à l'icône (remplace le noir d'un SVG, ou la couleur du glyphe).</param>
    /// <param name="pRenderScaling">Facteur d'échelle d'affichage, utile aux implémentations qui rasterisent.</param>
    IImage? GetIcon(string pIconName, double pSize, IBrush pForeground, double pRenderScaling = 1.0);
}
