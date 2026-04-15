using System.Collections.Generic;

namespace Corsair.Abstractions;

/// <summary>
/// A complete specification for exporting <typeparamref name="T"/> records
/// as a character-delimited flat file (CSV, TSV, pipe-separated, etc.).
/// </summary>
/// <typeparam name="T">The DTO type described by this spec.</typeparam>
public interface IDelimitedSpec<T> : ISpec<T>
{
    /// <summary>The character that separates field values on each line. Defaults to <c>','</c>.</summary>
    char Delimiter { get; }

    /// <summary>
    /// When <c>true</c> the exporter writes a header row — one column name per field —
    /// before any data rows.
    /// </summary>
    bool IncludeHeader { get; }

    /// <summary>The ordered list of field mappings that define each output column.</summary>
    IReadOnlyList<IDelimitedFieldSpec<T>> Fields { get; }
}
