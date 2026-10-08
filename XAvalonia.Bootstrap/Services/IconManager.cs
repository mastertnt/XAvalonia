using Avalonia.Media;
using XAvalonia.Shell.Abstractions.Icons;

namespace XAvalonia.Bootstrap.Services;

/// <summary>
/// Default <see cref="IIconManager"/> implementation.
/// Registered as singleton; plugins add providers during Phase 2 initialization.
/// </summary>
public sealed class IconManager : IIconManager
{
    private readonly List<IIconProvider> mProviders = new List<IIconProvider>();

    /// <inheritdoc/>
    public void RegisterProvider(IIconProvider pProvider)
    {
        if (!mProviders.Contains(pProvider))
        {
            mProviders.Add(pProvider);
        }
    }

    /// <inheritdoc/>
    public void UnregisterProvider(IIconProvider pProvider)
    {
        mProviders.Remove(pProvider);
    }

    /// <inheritdoc/>
    public IImage? GetIcon(string pIconName, double pSize, IBrush pForeground, double pRenderScaling = 1.0)
    {
        foreach (IIconProvider lProvider in mProviders)
        {
            if (lProvider.Contains(pIconName))
            {
                return lProvider.GetIcon(pIconName, pSize, pForeground, pRenderScaling);
            }
        }

        return null;
    }
}
