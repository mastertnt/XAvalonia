# IToolPanelService

Manages dockable tool panels placed in the left, right, bottom, or top zones of the shell layout. Tool panels are utility surfaces (explorer, properties, output, etc.) that complement the central document area.

---

## Interface

```csharp
public interface IToolPanelService
{
    void RegisterPanel(ToolPanelContribution pContribution);
    void UnregisterPanel(string pId);
}
```

---

## How to obtain

```csharp
IToolPanelService lToolPanelService = pServiceManager.RequestService<IToolPanelService>();
```

---

## ToolPanelContribution

```csharp
public sealed class ToolPanelContribution
{
    public ToolPanelContribution(
        string pId,
        string pTitle,
        object pViewModel,
        ToolPanelAlignment pAlignment)
}

public string Id { get; }                  // must match pViewModel.Id exactly
public string Title { get; }               // tab label in the tool strip
public object ViewModel { get; }           // must implement IDockable (extends Tool)
public ToolPanelAlignment Alignment { get; }
```

## ToolPanelAlignment

```csharp
public enum ToolPanelAlignment
{
    Left,
    Right,
    Bottom,
    Top
}
```

---

## ViewModel requirements

The ViewModel must extend `Dock.Model.ReactiveUI.Controls.Tool`:

```csharp
using Dock.Model.ReactiveUI.Controls;

public sealed class MyPanelViewModel : Tool
{
    public MyPanelViewModel()
    {
        Id    = "my-panel";   // stable, unique — must match ToolPanelContribution.Id
        Title = "My Panel";
        CanClose = true;
        CanPin   = true;
        CanFloat = true;
    }
}
```

---

## View resolution

The shell's `ViewLocator` resolves the view by replacing `ViewModel` with `View` in the full type name:

```
MyPlugin.ViewModels.MyPanelViewModel  →  MyPlugin.Views.MyPanelView
```

---

## Example — Explorer panel (left)

**ViewModel:**

```csharp
public sealed class ExplorerViewModel : Tool
{
    public ExplorerViewModel()
    {
        Id    = "explorer";
        Title = "Explorer";
        CanClose = true;
        CanPin   = true;
        CanFloat = true;
    }

    public ObservableCollection<FileNodeViewModel> Nodes { get; } = new();
}
```

**View (`ExplorerView.axaml`):**

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:MyPlugin.ViewModels"
             x:Class="MyPlugin.Views.ExplorerView"
             x:DataType="vm:ExplorerViewModel">

  <DockPanel>
    <TextBlock DockPanel.Dock="Top"
               Text="Explorer"
               Classes="panelHeader" />
    <TreeView ItemsSource="{Binding Nodes}">
      <TreeView.ItemTemplate>
        <TreeDataTemplate ItemsSource="{Binding Children}">
          <TextBlock Text="{Binding Name}" />
        </TreeDataTemplate>
      </TreeView.ItemTemplate>
    </TreeView>
  </DockPanel>
</UserControl>
```

**Registration:**

```csharp
lToolPanelService.RegisterPanel(new ToolPanelContribution(
    pId: "explorer",
    pTitle: "Explorer",
    pViewModel: new ExplorerViewModel(),
    pAlignment: ToolPanelAlignment.Left));
```

---

## Example — Properties panel (right)

```csharp
public sealed class PropertiesViewModel : Tool
{
    private string mSelectedName = "(none)";

    public PropertiesViewModel()
    {
        Id    = "properties";
        Title = "Properties";
        CanClose = true;
        CanPin   = true;
        CanFloat = true;
    }

    public string SelectedName
    {
        get => mSelectedName;
        set => this.RaiseAndSetIfChanged(ref mSelectedName, value);
    }
}
```

```csharp
lToolPanelService.RegisterPanel(new ToolPanelContribution(
    pId: "properties",
    pTitle: "Properties",
    pViewModel: new PropertiesViewModel(),
    pAlignment: ToolPanelAlignment.Right));
```

---

## Example — Output panel (bottom)

```csharp
public sealed class OutputViewModel : Tool
{
    private string mContent = string.Empty;

    public OutputViewModel()
    {
        Id    = "output";
        Title = "Output";
        CanClose = true;
        CanPin   = true;
        CanFloat = true;
    }

    public string Content
    {
        get => mContent;
        set => this.RaiseAndSetIfChanged(ref mContent, value);
    }

    public void AppendLine(string pLine)
    {
        Content += pLine + Environment.NewLine;
    }
}
```

```xml
<UserControl ...>
  <DockPanel>
    <TextBlock DockPanel.Dock="Top" Text="Output" Classes="panelHeader" />
    <ScrollViewer HorizontalScrollBarVisibility="Auto">
      <TextBlock Text="{Binding Content}"
                 FontFamily="Consolas,Menlo,monospace"
                 FontSize="12"
                 TextWrapping="NoWrap"
                 Margin="8" />
    </ScrollViewer>
  </DockPanel>
</UserControl>
```

```csharp
lToolPanelService.RegisterPanel(new ToolPanelContribution(
    pId: "output",
    pTitle: "Output",
    pViewModel: new OutputViewModel(),
    pAlignment: ToolPanelAlignment.Bottom));
```

---

## Example — multiple panels from one plugin

```csharp
public void Initialize(IPluginServiceManager pServiceManager)
{
    IToolPanelService lPanels = pServiceManager.RequestService<IToolPanelService>();

    lPanels.RegisterPanel(new ToolPanelContribution(
        "explorer", "Explorer", new ExplorerViewModel(), ToolPanelAlignment.Left));

    lPanels.RegisterPanel(new ToolPanelContribution(
        "properties", "Properties", new PropertiesViewModel(), ToolPanelAlignment.Right));

    lPanels.RegisterPanel(new ToolPanelContribution(
        "output", "Output", new OutputViewModel(), ToolPanelAlignment.Bottom));
}
```

---

## Example — unregistering a panel

```csharp
lToolPanelService.UnregisterPanel("output");
```

---

## Panel AXAML skeleton

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:MyPlugin.ViewModels"
             x:Class="MyPlugin.Views.MyPanelView"
             x:DataType="vm:MyPanelViewModel">

  <DockPanel>
    <!-- Optional header bar -->
    <TextBlock DockPanel.Dock="Top"
               Text="My Panel"
               Classes="panelHeader" />

    <!-- Panel content -->
    <Grid>
      <!-- … -->
    </Grid>
  </DockPanel>
</UserControl>
```

Use the `panelHeader` CSS class (defined in `AppStyles.axaml`) for a consistent header bar appearance.

---

## Summary

| Alignment | Typical use |
|---|---|
| `Left` | File explorer, project tree, navigation |
| `Right` | Properties, inspector, details |
| `Bottom` | Output, log, terminal, errors |
| `Top` | Rarely used; toolbars |
