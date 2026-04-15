using System;

namespace Corsair.Abstractions;

/// <summary>
/// Describes a single field mapping within a spec — common to both fixed-width
/// and delimited formats.
/// </summary>
/// <typeparam name="T">The DTO type that owns this field.</typeparam>
public interface IFieldSpec<T>
{
    /// <summary>The name of the mapped member (derived from the selector expression).</summary>
    string Name { get; }

    /// <summary>
    /// An optional composite or standard format string passed to
    /// <see cref="string.Format(string,object)"/> / <see cref="IFormattable.ToString(string,System.IFormatProvider)"/>
    /// when rendering the value. <c>null</c> means use the default <c>ToString()</c>.
    /// </summary>
    string? Format { get; }

    /// <summary>A compiled delegate that extracts the field value from a record instance.</summary>
    Func<T, object?> Getter { get; }

    /// <summary>
    /// Explicit column position used to sort fields before export.
    /// A value of <c>0</c> (the default) means no explicit position is assigned.
    /// Ordering is only applied when <b>every</b> field in the spec carries a
    /// non-zero value; if any field retains <c>0</c> the original declaration
    /// order is preserved for all fields.
    /// </summary>
    int Order { get; }
}
