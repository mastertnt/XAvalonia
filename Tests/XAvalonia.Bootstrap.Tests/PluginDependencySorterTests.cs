using Microsoft.Extensions.DependencyInjection;
using XAvalonia.Bootstrap.Plugins;
using XAvalonia.Shell.Abstractions.Logging;
using XAvalonia.Shell.Abstractions.Plugins;
using Xunit;

namespace XAvalonia.Bootstrap.Tests;

public sealed class PluginDependencySorterTests
{
    [Fact]
    public void Sort_KeepsInputOrder_WhenNoDependencies()
    {
        IPlugin[] lPlugins = { new PluginA(), new PluginB() };

        Assert.Equal(new[] { "a", "b" }, Ids(PluginDependencySorter.Sort(lPlugins)));
    }

    [Fact]
    public void Sort_PlacesPluginAfterDeclaredPluginDependency()
    {
        IPlugin[] lPlugins = { new DependsOnBPlugin(), new PluginA(), new PluginB() };

        Assert.Equal(new[] { "a", "b", "depends-on-b" }, Ids(PluginDependencySorter.Sort(lPlugins)));
    }

    [Fact]
    public void Sort_PlacesPluginAfterServiceProvider()
    {
        IPlugin[] lPlugins = { new LogConsumerPlugin(), new PluginA(), new LogProviderPlugin() };

        Assert.Equal(new[] { "a", "log-provider", "log-consumer" }, Ids(PluginDependencySorter.Sort(lPlugins)));
    }

    [Fact]
    public void Sort_IgnoresServiceWithoutProvider()
    {
        IPlugin[] lPlugins = { new LogConsumerPlugin(), new PluginA() };

        Assert.Equal(new[] { "log-consumer", "a" }, Ids(PluginDependencySorter.Sort(lPlugins)));
    }

    [Fact]
    public void Sort_HandlesTransitiveDependencies()
    {
        IPlugin[] lPlugins = { new DependsOnLogConsumerPlugin(), new LogConsumerPlugin(), new LogProviderPlugin() };

        Assert.Equal(
            new[] { "log-provider", "log-consumer", "depends-on-log-consumer" },
            Ids(PluginDependencySorter.Sort(lPlugins)));
    }

    [Fact]
    public void Sort_Throws_WhenRequiredPluginIsMissing()
    {
        IPlugin[] lPlugins = { new DependsOnBPlugin(), new PluginA() };

        PluginDependencyException lEx = Assert.Throws<PluginDependencyException>(() => PluginDependencySorter.Sort(lPlugins));
        Assert.Contains("'depends-on-b' depends on plugin 'b'", lEx.Message);
    }

    [Fact]
    public void Sort_IgnoresMissingOptionalPlugin()
    {
        IPlugin[] lPlugins = { new OptionalDependencyPlugin(), new PluginA() };

        Assert.Equal(new[] { "optional", "a" }, Ids(PluginDependencySorter.Sort(lPlugins)));
    }

    [Fact]
    public void Sort_HonorsOptionalPlugin_WhenPresent()
    {
        IPlugin[] lPlugins = { new OptionalDependencyPlugin(), new PluginB() };

        Assert.Equal(new[] { "b", "optional" }, Ids(PluginDependencySorter.Sort(lPlugins)));
    }

    [Fact]
    public void Sort_Throws_WithCyclePath()
    {
        IPlugin[] lPlugins = { new PluginA(), new CycleXPlugin(), new CycleYPlugin() };

        PluginDependencyException lEx = Assert.Throws<PluginDependencyException>(() => PluginDependencySorter.Sort(lPlugins));
        Assert.Contains("'cycle-x' -> 'cycle-y' -> 'cycle-x'", lEx.Message);
    }

    [Fact]
    public void Sort_Throws_WhenIdsAreDuplicated()
    {
        IPlugin[] lPlugins = { new PluginA(), new PluginA() };

        PluginDependencyException lEx = Assert.Throws<PluginDependencyException>(() => PluginDependencySorter.Sort(lPlugins));
        Assert.Contains("share the id 'a'", lEx.Message);
    }

    private static string[] Ids(IEnumerable<IPlugin> pPlugins) => pPlugins.Select(pPlugin => pPlugin.Id).ToArray();

    private abstract class TestPlugin : IPlugin
    {
        protected TestPlugin(string pId, params Type[] pProvidedServices)
        {
            Id = pId;
            ProvidedServices = pProvidedServices;
        }

        public string Id { get; }
        public string Name => Id;
        public string Description => Id;
        public Version CurrentVersion => new Version(1, 0, 0);
        public string? NuGetPackageId => null;
        public IReadOnlyList<Type> ProvidedServices { get; }
        public void RegisterServices(IServiceCollection pServices) { }
        public void Initialize(IPluginServiceManager pServiceManager) { }
    }

    private sealed class PluginA() : TestPlugin("a");

    private sealed class PluginB() : TestPlugin("b");

    [DependsOnPlugin("b")]
    private sealed class DependsOnBPlugin() : TestPlugin("depends-on-b");

    [DependsOnPlugin("b", Optional = true)]
    private sealed class OptionalDependencyPlugin() : TestPlugin("optional");

    private sealed class LogProviderPlugin() : TestPlugin("log-provider", typeof(ILogService));

    [DependsOnService(typeof(ILogService))]
    private sealed class LogConsumerPlugin() : TestPlugin("log-consumer");

    [DependsOnPlugin("log-consumer")]
    private sealed class DependsOnLogConsumerPlugin() : TestPlugin("depends-on-log-consumer");

    [DependsOnPlugin("cycle-y")]
    private sealed class CycleXPlugin() : TestPlugin("cycle-x");

    [DependsOnPlugin("cycle-x")]
    private sealed class CycleYPlugin() : TestPlugin("cycle-y");
}
