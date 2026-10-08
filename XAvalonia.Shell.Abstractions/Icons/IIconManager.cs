using Avalonia.Media;

namespace XAvalonia.Shell.Abstractions.Icons;

/// <summary>
/// Aggregates <see cref="IIconProvider"/> instances registered by plugins and resolves icons by name.
/// Providers are queried in registration order; the first match wins.
/// </summary>
public interface IIconManager
{
    /// <summary>Adds a provider to the resolution chain.</summary>
    /// <param name="pProvider">The provider to register.</param>
    void RegisterProvider(IIconProvider pProvider);

    /// <summary>Removes a previously registered provider.</summary>
    /// <param name="pProvider">The provider to remove.</param>
    void UnregisterProvider(IIconProvider pProvider);

    /// <summary>
    /// Returns the icon from the first provider that contains <paramref name="pIconName"/>,
    /// or <c>null</c> if no registered provider can supply it.
    /// </summary>
    /// <param name="pIconName">Logical icon identifier (e.g. "Save", "Folder").</param>
    /// <param name="pSize">Desired size in device-independent units (square icon).</param>
    /// <param name="pForeground">Brush applied to the icon.</param>
    /// <param name="pRenderScaling">Display scale factor; useful for rasterising implementations.</param>
    IImage? GetIcon(string pIconName, double pSize, IBrush pForeground, double pRenderScaling = 1.0);
}
