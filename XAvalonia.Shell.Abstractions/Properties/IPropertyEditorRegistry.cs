namespace XAvalonia.Shell.Abstractions.Properties;

/// <summary>
/// Maps value types to the <see cref="IPropertyEditor"/> used by property inspectors.
/// The shell registers editors for <see cref="bool"/>, <see cref="int"/>, <see cref="double"/>,
/// <see cref="string"/> and enums; plugins add or replace editors for their own types.
/// </summary>
/// <example>
/// <code>
/// IPropertyEditorRegistry lRegistry = pServiceManager.RequestService&lt;IPropertyEditorRegistry&gt;();
/// lRegistry.Register(typeof(Color), new ColorPropertyEditor());
/// </code>
/// </example>
public interface IPropertyEditorRegistry
{
    /// <summary>Raised after an editor is registered or removed, so that open inspectors rebuild their rows.</summary>
    event EventHandler? EditorsChanged;

    /// <summary>
    /// Registers <paramref name="pEditor"/> for <paramref name="pValueType"/>, replacing any previous one.
    /// Registering a base type or interface (e.g. <see cref="Enum"/>) covers every derived type
    /// that has no editor of its own.
    /// </summary>
    void Register(Type pValueType, IPropertyEditor pEditor);

    /// <summary>Removes the editor registered for exactly <paramref name="pValueType"/>.</summary>
    /// <returns><c>true</c> if an editor was removed.</returns>
    bool Unregister(Type pValueType);

    /// <summary>
    /// Returns the editor for <paramref name="pValueType"/>: the one registered for the type itself,
    /// then for its nullable underlying type, its base types and its interfaces, in that order;
    /// <c>null</c> when none applies.
    /// </summary>
    IPropertyEditor? Find(Type pValueType);
}
