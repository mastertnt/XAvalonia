using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Bootstrap.Services;
using XAvalonia.Shell.Abstractions.Plugins;
using XAvalonia.Shell.Abstractions.Shell;
using Xunit;

namespace XAvalonia.Bootstrap.Tests;

public sealed class UserConfigurationServiceTests : IDisposable
{
    private readonly string mDirectory = Path.Combine(Path.GetTempPath(), "xavalonia-tests-" + Guid.NewGuid().ToString("N"));

    private string FilePath => Path.Combine(mDirectory, "user_configuration.json");

    public void Dispose()
    {
        if (Directory.Exists(mDirectory))
        {
            Directory.Delete(mDirectory, true);
        }
    }

    [Fact]
    public void RegisterPlugin_DescribesSettingsAndAppliesFileValues()
    {
        WriteFile("""{ "Editor": { "fontSize": 18, "wrapping": "wrap" } }""");
        UserConfigurationService lService = new UserConfigurationService(FilePath);
        EditorPlugin lPlugin = new EditorPlugin();

        lService.RegisterPlugin(lPlugin);

        Assert.Equal(18, lPlugin.FontSize);
        Assert.Equal(Wrapping.Wrap, lPlugin.Wrapping);
        Assert.Equal(3, lService.Settings.Count);

        UserSettingDescriptor lFontSize = Get(lService, "fontSize");
        Assert.Equal("Editor.fontSize", lFontSize.FullKey);
        Assert.Equal("Font Size", lFontSize.DisplayName);
        Assert.Equal("Text Editor/Font", lFontSize.Category);
        Assert.Equal(13.0, lFontSize.DefaultValue);
        Assert.True(lService.IsModified(lFontSize));
        Assert.Equal("Editor Plugin", Get(lService, "showLineNumbers").Category);
        Assert.False(lService.IsModified(Get(lService, "showLineNumbers")));
    }

    [Fact]
    public void SetValue_UpdatesPluginWritesFileAndRaisesEvent()
    {
        UserConfigurationService lService = new UserConfigurationService(FilePath);
        EditorPlugin lPlugin = new EditorPlugin();
        lService.RegisterPlugin(lPlugin);
        UserSettingChangedEventArgs? lArgs = null;
        lService.SettingChanged += (_, pArgs) => lArgs = pArgs;

        lService.SetValue(Get(lService, "fontSize"), 20m);
        lService.SetValue(Get(lService, "wrapping"), Wrapping.Wrap);

        Assert.Equal(20, lPlugin.FontSize);
        Assert.NotNull(lArgs);
        Assert.Equal(Wrapping.NoWrap, lArgs!.OldValue);
        Assert.Equal(Wrapping.Wrap, lArgs.NewValue);

        JsonNode lSaved = JsonNode.Parse(File.ReadAllText(FilePath))!;
        Assert.Equal(20.0, lSaved["Editor"]!["fontSize"]!.GetValue<double>());
        Assert.Equal("Wrap", lSaved["Editor"]!["wrapping"]!.GetValue<string>());
    }

    [Fact]
    public void SetValue_ClampsToBounds()
    {
        UserConfigurationService lService = new UserConfigurationService(FilePath);
        EditorPlugin lPlugin = new EditorPlugin();
        lService.RegisterPlugin(lPlugin);

        lService.SetValue(Get(lService, "fontSize"), 500.0);

        Assert.Equal(72, lPlugin.FontSize);
    }

    [Fact]
    public void ResetValue_RestoresDefaultAndRemovesKey()
    {
        WriteFile("""{ "Editor": { "fontSize": 18 }, "Other": { "keep": true } }""");
        UserConfigurationService lService = new UserConfigurationService(FilePath);
        EditorPlugin lPlugin = new EditorPlugin();
        lService.RegisterPlugin(lPlugin);

        lService.ResetValue(Get(lService, "fontSize"));

        Assert.Equal(13, lPlugin.FontSize);
        Assert.False(lService.IsModified(Get(lService, "fontSize")));
        JsonNode lSaved = JsonNode.Parse(File.ReadAllText(FilePath))!;
        Assert.Null(lSaved["Editor"]);
        Assert.True(lSaved["Other"]!["keep"]!.GetValue<bool>());
    }

    [Fact]
    public void MalformedFile_IsNeverOverwritten()
    {
        WriteFile("{ not json");
        UserConfigurationService lService = new UserConfigurationService(FilePath);
        EditorPlugin lPlugin = new EditorPlugin();
        lService.RegisterPlugin(lPlugin);

        Assert.Throws<IOException>(() => lService.SetValue(Get(lService, "fontSize"), 20.0));

        Assert.Equal(20, lPlugin.FontSize);
        Assert.Equal("{ not json", File.ReadAllText(FilePath));
    }

    [Fact]
    public void UnregisterPlugin_RemovesItsSettings()
    {
        UserConfigurationService lService = new UserConfigurationService(FilePath);
        EditorPlugin lPlugin = new EditorPlugin();
        lService.RegisterPlugin(lPlugin);

        lService.UnregisterPlugin(lPlugin.Id);

        Assert.Empty(lService.Settings);
    }

    [Fact]
    public void StringConvertibleTypes_AreReadAndWrittenAsStrings()
    {
        WriteFile("""{ "Theme": { "accent": "#FF102030", "delay": "00:00:05" } }""");
        UserConfigurationService lService = new UserConfigurationService(FilePath);
        ThemePlugin lPlugin = new ThemePlugin();
        lService.RegisterPlugin(lPlugin);

        Assert.Equal(Avalonia.Media.Color.Parse("#FF102030"), lPlugin.Accent);
        Assert.Equal(TimeSpan.FromSeconds(5), lPlugin.Delay);

        lService.SetValue(Get(lService, "accent"), Avalonia.Media.Colors.Red);
        lService.SetValue(Get(lService, "delay"), "00:01:00");

        Assert.Equal(TimeSpan.FromMinutes(1), lPlugin.Delay);
        JsonNode lSaved = JsonNode.Parse(File.ReadAllText(FilePath))!;
        Assert.Equal(Avalonia.Media.Colors.Red, Avalonia.Media.Color.Parse(lSaved["Theme"]!["accent"]!.GetValue<string>()));
        Assert.Equal("00:01:00", lSaved["Theme"]!["delay"]!.GetValue<string>());
        Assert.Throws<ArgumentException>(() => lService.SetValue(Get(lService, "accent"), "not a color"));
    }

    [Theory]
    [InlineData("fontSize", "Font Size")]
    [InlineData("editor.word_wrap", "Editor Word Wrap")]
    [InlineData("URL", "URL")]
    public void Humanize_SplitsKeys(string pKey, string pExpected)
    {
        Assert.Equal(pExpected, UserConfigurationService.Humanize(pKey));
    }

    private static UserSettingDescriptor Get(UserConfigurationService pService, string pKey)
        => pService.Settings.Single(pS => pS.Key == pKey);

    private void WriteFile(string pContent)
    {
        Directory.CreateDirectory(mDirectory);
        File.WriteAllText(FilePath, pContent);
    }

    public enum Wrapping { NoWrap, Wrap }

    private sealed class ThemePlugin : IPlugin
    {
        [UserConfiguration("accent")]
        public Avalonia.Media.Color Accent { get; set; } = Avalonia.Media.Colors.Blue;

        [UserConfiguration("delay")]
        public TimeSpan Delay { get; set; } = TimeSpan.FromSeconds(1);

        public string Id => "test.theme";
        public string Name => "Theme";
        public string Description => string.Empty;
        public Version CurrentVersion => new Version(1, 0);
        public string? NuGetPackageId => null;
        public IReadOnlyList<Type> ProvidedServices => Array.Empty<Type>();
        public void RegisterServices(IServiceCollection pServices) { }
        public void Initialize(IPluginServiceManager pServiceManager) { }
    }

    [ConfigurationSection("Editor")]
    private sealed class EditorPlugin : IPlugin
    {
        [UserConfiguration("fontSize", Category = "Text Editor/Font", Minimum = 6, Maximum = 72)]
        public double FontSize { get; set; } = 13;

        [UserConfiguration("wrapping", Category = "Text Editor")]
        public Wrapping Wrapping { get; set; } = Wrapping.NoWrap;

        [UserConfiguration("showLineNumbers")]
        public bool ShowLineNumbers { get; set; } = true;

        public string Id => "test.editor";
        public string Name => "Editor Plugin";
        public string Description => string.Empty;
        public Version CurrentVersion => new Version(1, 0);
        public string? NuGetPackageId => null;
        public IReadOnlyList<Type> ProvidedServices => Array.Empty<Type>();
        public void RegisterServices(IServiceCollection pServices) { }
        public void Initialize(IPluginServiceManager pServiceManager) { }
    }
}
