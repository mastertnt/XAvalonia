namespace XAvalonia.Splashscreen;

/// <summary>Appearance and behaviour of the splash screen, read from the technical configuration.</summary>
internal sealed class SplashscreenOptions
{
    /// <summary>Image to display: <c>avares://</c> URI, absolute path, or path relative to the application directory.</summary>
    public string? ImagePath { get; init; }

    /// <summary>Window width in pixels; <c>null</c> to use the image width.</summary>
    public int? Width { get; init; }

    /// <summary>Window height in pixels; <c>null</c> to use the image height.</summary>
    public int? Height { get; init; }

    /// <summary>Window background color (e.g. <c>#1E1E1E</c>), visible around or without the image.</summary>
    public string Background { get; init; } = "#1E1E1E";

    /// <summary>Message text color.</summary>
    public string Foreground { get; init; } = "#FFFFFF";

    /// <summary>Minimum time the splash screen stays visible, even if the main window is ready earlier.</summary>
    public TimeSpan MinimumDisplayTime { get; init; } = TimeSpan.Zero;
}
