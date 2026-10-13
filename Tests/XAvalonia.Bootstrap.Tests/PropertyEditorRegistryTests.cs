using Avalonia.Controls;
using XAvalonia.Controls;
using XAvalonia.Controls.Editors;
using XAvalonia.Shell.Abstractions.Properties;
using Xunit;

namespace XAvalonia.Bootstrap.Tests;

public sealed class PropertyEditorRegistryTests
{
    [Fact]
    public void Find_ReturnsBuiltInEditors()
    {
        PropertyEditorRegistry lRegistry = new PropertyEditorRegistry();

        Assert.IsType<BooleanPropertyEditor>(lRegistry.Find(typeof(bool)));
        Assert.IsType<BooleanPropertyEditor>(lRegistry.Find(typeof(bool?)));
        Assert.IsType<NumericPropertyEditor>(lRegistry.Find(typeof(int)));
        Assert.IsType<NumericPropertyEditor>(lRegistry.Find(typeof(double?)));
        Assert.IsType<TextPropertyEditor>(lRegistry.Find(typeof(string)));
        Assert.IsType<EnumPropertyEditor>(lRegistry.Find(typeof(DayOfWeek)));
        Assert.Null(lRegistry.Find(typeof(TimeSpan)));
    }

    [Fact]
    public void Register_AddsOrReplacesEditorAndRaisesEvent()
    {
        PropertyEditorRegistry lRegistry = new PropertyEditorRegistry();
        int lChanges = 0;
        lRegistry.EditorsChanged += (_, _) => lChanges++;
        IPropertyEditor lCustom = new StubEditor();

        lRegistry.Register<TimeSpan>(lCustom);
        lRegistry.Register(typeof(DayOfWeek), lCustom);

        Assert.Same(lCustom, lRegistry.Find(typeof(TimeSpan?)));
        Assert.Same(lCustom, lRegistry.Find(typeof(DayOfWeek)));
        Assert.IsType<EnumPropertyEditor>(lRegistry.Find(typeof(ConsoleColor)));
        Assert.Equal(2, lChanges);

        Assert.True(lRegistry.Unregister(typeof(DayOfWeek)));
        Assert.IsType<EnumPropertyEditor>(lRegistry.Find(typeof(DayOfWeek)));
    }

    [Fact]
    public void Find_UsesBaseTypesThenInterfaces()
    {
        PropertyEditorRegistry lRegistry = new PropertyEditorRegistry();
        IPropertyEditor lForBase = new StubEditor();
        IPropertyEditor lForInterface = new StubEditor();

        lRegistry.Register(typeof(Exception), lForBase);
        lRegistry.Register(typeof(IComparable), lForInterface);

        Assert.Same(lForBase, lRegistry.Find(typeof(InvalidOperationException)));
        Assert.Same(lForInterface, lRegistry.Find(typeof(Version)));
    }

    [Theory]
    [InlineData("Text Editor/Font", "Text Editor", true)]
    [InlineData("Text Editor/Font", "text editor/font", true)]
    [InlineData("Text Editor Extra", "Text Editor", false)]
    [InlineData("Log Viewer", "", true)]
    public void IsInCategory_MatchesPathAndChildren(string pCategory, string pPath, bool pExpected)
    {
        Assert.Equal(pExpected, PropertyItemFilter.IsInCategory(pCategory, pPath));
    }

    private sealed class StubEditor : IPropertyEditor
    {
        public Control CreateEditor(IPropertyItem pItem) => new TextBlock();
    }
}
