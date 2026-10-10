using XAvalonia.Bootstrap.Services;
using XAvalonia.Shell.Abstractions.Selection;
using Xunit;

namespace XAvalonia.Bootstrap.Tests;

public sealed class SelectionManagerTests
{
    [Fact]
    public void NewManager_HasActiveEmptyGlobalContext()
    {
        SelectionManager lManager = new SelectionManager();

        Assert.Equal(ISelectionManager.GlobalContextId, lManager.GlobalContext.Id);
        Assert.Same(lManager.GlobalContext, lManager.ActiveContext);
        Assert.True(lManager.GlobalContext.IsEmpty);
        Assert.Single(lManager.Contexts);
    }

    [Fact]
    public void Select_ReplacesSelectionAndReportsDifference()
    {
        SelectionContext lContext = new SelectionContext("c");
        lContext.Select(new object[] { "a", "b" });
        SelectionChangedEventArgs? lArgs = null;
        lContext.SelectionChanged += (_, pArgs) => lArgs = pArgs;

        lContext.Select(new object[] { "b", "c" });

        Assert.Equal(new object[] { "b", "c" }, lContext.SelectedItems);
        Assert.Equal("c", lContext.PrimaryItem);
        Assert.NotNull(lArgs);
        Assert.Equal(new object[] { "c" }, lArgs!.AddedItems);
        Assert.Equal(new object[] { "a" }, lArgs.RemovedItems);
    }

    [Fact]
    public void Select_IgnoresDuplicates()
    {
        SelectionContext lContext = new SelectionContext("c");

        lContext.Select(new object[] { "a", "a", "b" });

        Assert.Equal(new object[] { "a", "b" }, lContext.SelectedItems);
    }

    [Fact]
    public void Operations_DoNotRaiseEvent_WhenSelectionUnchanged()
    {
        SelectionContext lContext = new SelectionContext("c");
        lContext.Select("a");
        int lCount = 0;
        lContext.SelectionChanged += (_, _) => lCount++;

        lContext.Select("a");
        lContext.Add("a");
        lContext.Remove("z");

        Assert.Equal(0, lCount);
    }

    [Fact]
    public void AddRemoveToggleClear_UpdateSelection()
    {
        SelectionContext lContext = new SelectionContext("c");

        lContext.Add("a");
        lContext.Add("b");
        lContext.Toggle("a");
        lContext.Toggle("c");

        Assert.Equal(new object[] { "b", "c" }, lContext.SelectedItems);
        Assert.True(lContext.IsSelected("b"));
        Assert.False(lContext.IsSelected("a"));

        lContext.Clear();

        Assert.True(lContext.IsEmpty);
        Assert.Null(lContext.PrimaryItem);
    }

    [Fact]
    public void GetSelected_FiltersByType()
    {
        SelectionContext lContext = new SelectionContext("c");
        lContext.Select(new object[] { "a", 1, "b" });

        Assert.Equal(new[] { "a", "b" }, lContext.GetSelected<string>());
    }

    [Fact]
    public void Select_RejectsNull()
    {
        SelectionContext lContext = new SelectionContext("c");

        Assert.Throws<ArgumentNullException>(() => lContext.Select(new object[] { "a", null! }));
        Assert.True(lContext.IsEmpty);
    }

    [Fact]
    public void GetOrCreateContext_ReturnsSameInstanceForSameId()
    {
        SelectionManager lManager = new SelectionManager();

        ISelectionContext lFirst = lManager.GetOrCreateContext("doc1");
        ISelectionContext lSecond = lManager.GetOrCreateContext("doc1");

        Assert.Same(lFirst, lSecond);
        Assert.Same(lManager.GlobalContext, lManager.GetOrCreateContext(ISelectionManager.GlobalContextId));
        Assert.Equal(2, lManager.Contexts.Count);
        Assert.Null(lManager.GetContext("unknown"));
    }

    [Fact]
    public void ContextsAreIndependent()
    {
        SelectionManager lManager = new SelectionManager();
        ISelectionContext lLocal = lManager.GetOrCreateContext("doc1");

        lManager.GlobalContext.Select("g");
        lLocal.Select("l");

        Assert.Equal(new object[] { "g" }, lManager.GlobalContext.SelectedItems);
        Assert.Equal(new object[] { "l" }, lLocal.SelectedItems);
    }

    [Fact]
    public void ActiveSelectionChanged_RelaysOnlyActiveContext()
    {
        SelectionManager lManager = new SelectionManager();
        ISelectionContext lLocal = lManager.GetOrCreateContext("doc1");
        List<SelectionChangedEventArgs> lEvents = new();
        lManager.ActiveSelectionChanged += (_, pArgs) => lEvents.Add(pArgs);

        lLocal.Select("l");
        lManager.GlobalContext.Select("g");

        SelectionChangedEventArgs lArgs = Assert.Single(lEvents);
        Assert.Same(lManager.GlobalContext, lArgs.Context);
    }

    [Fact]
    public void ActivateContext_RaisesEventsWithSelectionDifference()
    {
        SelectionManager lManager = new SelectionManager();
        lManager.GlobalContext.Select(new object[] { "shared", "g" });
        ISelectionContext lLocal = lManager.GetOrCreateContext("doc1");
        lLocal.Select(new object[] { "shared", "l" });
        ActiveSelectionContextChangedEventArgs? lActiveArgs = null;
        SelectionChangedEventArgs? lSelectionArgs = null;
        lManager.ActiveContextChanged += (_, pArgs) => lActiveArgs = pArgs;
        lManager.ActiveSelectionChanged += (_, pArgs) => lSelectionArgs = pArgs;

        lManager.ActivateContext(lLocal);

        Assert.Same(lLocal, lManager.ActiveContext);
        Assert.NotNull(lActiveArgs);
        Assert.Same(lManager.GlobalContext, lActiveArgs!.PreviousContext);
        Assert.Same(lLocal, lActiveArgs.CurrentContext);
        Assert.NotNull(lSelectionArgs);
        Assert.Equal(new object[] { "l" }, lSelectionArgs!.AddedItems);
        Assert.Equal(new object[] { "g" }, lSelectionArgs.RemovedItems);

        lSelectionArgs = null;
        lLocal.Add("x");
        Assert.NotNull(lSelectionArgs);
        Assert.Same(lLocal, lSelectionArgs!.Context);
    }

    [Fact]
    public void ActivateContext_ById_CreatesContext()
    {
        SelectionManager lManager = new SelectionManager();

        ISelectionContext lContext = lManager.ActivateContext("panel");

        Assert.Equal("panel", lContext.Id);
        Assert.Same(lContext, lManager.ActiveContext);
    }

    [Fact]
    public void ActivateContext_SameContext_DoesNothing()
    {
        SelectionManager lManager = new SelectionManager();
        int lCount = 0;
        lManager.ActiveContextChanged += (_, _) => lCount++;

        lManager.ActivateGlobalContext();

        Assert.Equal(0, lCount);
    }

    [Fact]
    public void ActivateContext_RejectsForeignContext()
    {
        SelectionManager lManager = new SelectionManager();
        ISelectionContext lForeign = new SelectionManager().GetOrCreateContext("doc1");

        Assert.Throws<ArgumentException>(() => lManager.ActivateContext(lForeign));
    }

    [Fact]
    public void RemoveContext_ReactivatesGlobalAndStopsRelaying()
    {
        SelectionManager lManager = new SelectionManager();
        ISelectionContext lLocal = lManager.ActivateContext("doc1");
        int lCount = 0;
        lManager.ActiveSelectionChanged += (_, _) => lCount++;

        Assert.True(lManager.RemoveContext("doc1"));
        lLocal.Select("l");

        Assert.Same(lManager.GlobalContext, lManager.ActiveContext);
        Assert.Equal(0, lCount);
        Assert.Null(lManager.GetContext("doc1"));
        Assert.False(lManager.RemoveContext("doc1"));
        Assert.Throws<ArgumentException>(() => lManager.ActivateContext(lLocal));
    }

    [Fact]
    public void RemoveContext_RejectsGlobal()
    {
        SelectionManager lManager = new SelectionManager();

        Assert.Throws<InvalidOperationException>(() => lManager.RemoveContext(ISelectionManager.GlobalContextId));
    }
}
