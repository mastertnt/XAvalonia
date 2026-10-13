using XAvalonia.Controls.Editors;
using XAvalonia.Shell.Abstractions.Properties;

namespace XAvalonia.Controls;

/// <summary>
/// Default <see cref="IPropertyEditorRegistry"/>, pre-filled with editors for
/// <see cref="bool"/>, <see cref="int"/>, <see cref="double"/>, <see cref="string"/> and enums.
/// The shell registers one instance as a singleton; plugins add editors for their own types.
/// </summary>
public sealed class PropertyEditorRegistry : IPropertyEditorRegistry
{
    private readonly Dictionary<Type, IPropertyEditor> mEditors = new Dictionary<Type, IPropertyEditor>();

    /// <summary>Initializes the registry with the built-in editors.</summary>
    public PropertyEditorRegistry()
    {
        mEditors[typeof(bool)]   = new BooleanPropertyEditor();
        mEditors[typeof(int)]    = new NumericPropertyEditor();
        mEditors[typeof(double)] = mEditors[typeof(int)];
        mEditors[typeof(string)] = new TextPropertyEditor();
        mEditors[typeof(Enum)]   = new EnumPropertyEditor();
    }

    /// <summary>Editor used when no registered editor applies: shows the value as read-only text.</summary>
    public static IPropertyEditor Fallback { get; } = new ReadOnlyPropertyEditor();

    /// <inheritdoc/>
    public event EventHandler? EditorsChanged;

    /// <inheritdoc/>
    public void Register(Type pValueType, IPropertyEditor pEditor)
    {
        mEditors[pValueType] = pEditor;
        EditorsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public bool Unregister(Type pValueType)
    {
        bool lRemoved = mEditors.Remove(pValueType);
        if (lRemoved)
        {
            EditorsChanged?.Invoke(this, EventArgs.Empty);
        }

        return lRemoved;
    }

    /// <inheritdoc/>
    public IPropertyEditor? Find(Type pValueType)
    {
        if (mEditors.TryGetValue(pValueType, out IPropertyEditor? lEditor))
        {
            return lEditor;
        }

        Type lType = Nullable.GetUnderlyingType(pValueType) ?? pValueType;
        for (Type? lCurrent = lType; lCurrent is not null; lCurrent = lCurrent.BaseType)
        {
            if (mEditors.TryGetValue(lCurrent, out lEditor))
            {
                return lEditor;
            }
        }

        foreach (Type lInterface in lType.GetInterfaces())
        {
            if (mEditors.TryGetValue(lInterface, out lEditor))
            {
                return lEditor;
            }
        }

        return null;
    }
}
