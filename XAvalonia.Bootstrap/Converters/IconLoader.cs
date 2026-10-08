using System.Diagnostics;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace XAvalonia.Bootstrap.Converters;

/// <summary>
/// Utility for loading Avalonia-compatible bitmaps from URI strings.
/// Centralises icon loading so that both menu and status bar view models
/// share the same resolution logic.
/// </summary>
internal static class IconLoader
{
    /// <summary>
    /// Loads a <see cref="Bitmap"/> from a URI string.
    /// </summary>
    /// <param name="pUri">
    /// Asset URI (<c>avares://assembly/path</c>) or absolute file path.
    /// Returns <c>null</c> when <paramref name="pUri"/> is <c>null</c> or empty.
    /// </param>
    /// <returns>The loaded bitmap, or <c>null</c> if the resource cannot be found or parsed.</returns>
    internal static Bitmap? Load(string? pUri)
    {
        if (string.IsNullOrEmpty(pUri))
        {
            return null;
        }

        try
        {
            Uri lUri = new Uri(pUri, UriKind.RelativeOrAbsolute);

            if (lUri.Scheme == "avares")
            {
                return new Bitmap(AssetLoader.Open(lUri));
            }

            return new Bitmap(pUri);
        }
        catch (Exception lEx)
        {
            Trace.WriteLine($"[IconLoader] Failed to load icon from '{pUri}': {lEx.Message}");
            return null;
        }
    }
}
