namespace Corsair.Abstractions;

/// <summary>
/// Describes a single field mapping for a delimited flat file, adding
/// an optional header label on top of the common <see cref="IFieldSpec{T}"/>.
/// </summary>
/// <typeparam name="T">The DTO type that owns this field.</typeparam>
public interface IDelimitedFieldSpec<T> : IFieldSpec<T>
{
    /// <summary>
    /// The column header written when the spec requests a header row.
    /// Defaults to <see cref="IFieldSpec{T}.Name"/> when not explicitly set.
    /// </summary>
    string Header { get; }
}
