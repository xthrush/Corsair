using System.Collections.Generic;

namespace Corsair.Abstractions;

/// <summary>
/// A complete specification for exporting <typeparamref name="T"/> records
/// as a fixed-width flat file.
/// </summary>
/// <typeparam name="T">The DTO type described by this spec.</typeparam>
public interface IFixedSpec<T> : ISpec<T>
{
    /// <summary>The ordered list of field mappings that define each output column.</summary>
    IReadOnlyList<IFixedFieldSpec<T>> Fields { get; }
}
