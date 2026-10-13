using XAvalonia.Bootstrap.Converters;
using Xunit;

namespace XAvalonia.Bootstrap.Tests;

public sealed class IconLoaderTests
{
    [Fact]
    public void TryGetAssetUri_AvaresUri_ReturnsUri()
    {
        Uri? lUri = IconLoader.TryGetAssetUri("avares://My.App/Assets/icon.png");

        Assert.NotNull(lUri);
        Assert.Equal("avares", lUri!.Scheme);
    }

    [Fact]
    public void TryGetAssetUri_RootedFilePath_ReturnsNull()
    {
        string lPath = Path.Combine(Path.GetTempPath(), "icon.png");

        Assert.Null(IconLoader.TryGetAssetUri(lPath));
    }

    [Theory]
    [InlineData("icon.png")]
    [InlineData("Assets/icon.png")]
    public void TryGetAssetUri_RelativeFilePath_ReturnsNull(string pPath)
    {
        Assert.Null(IconLoader.TryGetAssetUri(pPath));
    }

    [Fact]
    public void ToFilePath_RootedFilePath_IsKept()
    {
        string lPath = Path.Combine(Path.GetTempPath(), "icon.png");

        Assert.Equal(lPath, IconLoader.ToFilePath(lPath));
    }

    [Fact]
    public void ToFilePath_FileUri_ReturnsLocalPath()
    {
        string lPath = Path.Combine(Path.GetTempPath(), "icon.png");

        Assert.Equal(lPath, IconLoader.ToFilePath(new Uri(lPath).AbsoluteUri));
    }

    [Fact]
    public void ToFilePath_RelativeFilePath_IsKept()
    {
        Assert.Equal("Assets/icon.png", IconLoader.ToFilePath("Assets/icon.png"));
    }

    [Fact]
    public void Load_MissingFile_ReturnsNull()
    {
        string lPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");

        Assert.Null(IconLoader.Load(lPath));
    }
}
