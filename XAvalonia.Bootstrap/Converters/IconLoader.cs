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
    /// Asset URI (<c>avares://assembly/path</c>), <c>file://</c> URI, or file path.
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
            Uri? lAssetUri = TryGetAssetUri(pUri);
            if (lAssetUri is not null)
            {
                return new Bitmap(AssetLoader.Open(lAssetUri));
            }

            return new Bitmap(ToFilePath(pUri));
        }
        catch (Exception lEx)
        {
            Trace.WriteLine($"[IconLoader] Failed to load icon from '{pUri}': {lEx.Message}");
            return null;
        }
    }

    /// <summary>Returns the parsed URI when <paramref name="pUri"/> is an <c>avares://</c> asset URI, otherwise <c>null</c>.</summary>
    internal static Uri? TryGetAssetUri(string pUri)
    {
        return Uri.TryCreate(pUri, UriKind.Absolute, out Uri? lUri) && lUri.Scheme == "avares" ? lUri : null;
    }

    /// <summary>
    /// Converts a <c>file://</c> URI to a local path; any other value is returned unchanged.
    /// A rooted path parsed as <see cref="UriKind.RelativeOrAbsolute"/> is a relative URI on Unix
    /// (its <see cref="Uri.Scheme"/> throws), so only absolute URIs are inspected here.
    /// </summary>
    internal static string ToFilePath(string pUri)
    {
        return Uri.TryCreate(pUri, UriKind.Absolute, out Uri? lUri) && lUri.IsFile ? lUri.LocalPath : pUri;
    }
}
