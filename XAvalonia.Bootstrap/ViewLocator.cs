using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Dock.Model.ReactiveUI.Controls;
using XAvalonia.Bootstrap.ViewModels;

namespace XAvalonia.Bootstrap;

/// <summary>
/// Resolves a View type from a ViewModel type by convention (replacing "ViewModel" with "View").
/// Searches across all loaded assemblies so that plugin views are found at runtime.
/// </summary>
public class ViewLocator : IDataTemplate
{
    /// <summary>Builds the view for <paramref name="pData"/> by convention.</summary>
    public Control? Build(object? pData)
    {
        if (pData is null)
        {
            return null;
        }

        string lName = pData.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);

        foreach (Assembly lAssembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? lType = lAssembly.GetType(lName);
            if (lType is not null)
            {
                return (Control)Activator.CreateInstance(lType)!;
            }
        }

        return new TextBlock { Text = $"View not found: {lName}" };
    }

    /// <summary>
    /// Returns <c>true</c> for <see cref="ViewModelBase"/> instances and for Dock
    /// <see cref="Tool"/> and <see cref="Document"/> types so that plugin panel views
    /// are resolved by naming convention.
    /// </summary>
    public bool Match(object? pData) => pData is ViewModelBase or Tool or Document;
}
