using Avalonia.Controls;

namespace XAvalonia.Shell.Abstractions.Properties;

/// <summary>Convenience overloads for <see cref="IPropertyEditorRegistry"/>.</summary>
public static class PropertyEditorRegistryExtensions
{
    /// <summary>Registers <paramref name="pEditor"/> for <typeparamref name="T"/>.</summary>
    public static void Register<T>(this IPropertyEditorRegistry pRegistry, IPropertyEditor pEditor)
        => pRegistry.Register(typeof(T), pEditor);

    /// <summary>Registers an editor built by <paramref name="pFactory"/> for <typeparamref name="T"/>.</summary>
    public static void Register<T>(this IPropertyEditorRegistry pRegistry, Func<IPropertyItem, Control> pFactory)
        => pRegistry.Register(typeof(T), new DelegatePropertyEditor(pFactory));

    private sealed class DelegatePropertyEditor : IPropertyEditor
    {
        private readonly Func<IPropertyItem, Control> mFactory;

        public DelegatePropertyEditor(Func<IPropertyItem, Control> pFactory)
        {
            mFactory = pFactory;
        }

        public Control CreateEditor(IPropertyItem pItem) => mFactory(pItem);
    }
}
