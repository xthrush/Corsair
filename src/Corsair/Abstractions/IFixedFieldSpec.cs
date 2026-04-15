namespace Corsair.Abstractions;

/// <summary>
/// Describes a single field mapping for a fixed-width flat file, adding
/// width, alignment, and padding on top of the common <see cref="IFieldSpec{T}"/>.
/// </summary>
/// <typeparam name="T">The DTO type that owns this field.</typeparam>
public interface IFixedFieldSpec<T> : IFieldSpec<T>
{
    /// <summary>The exact number of characters this field occupies in the output.</summary>
    int Width { get; }

    /// <summary>How the rendered value is aligned within the field boundary.</summary>
    FieldAlignment Alignment { get; }

    /// <summary>
    /// The character used to pad the field to its full <see cref="Width"/>.
    /// Defaults to <c>' '</c> (space).
    /// </summary>
    char Padding { get; }
}
