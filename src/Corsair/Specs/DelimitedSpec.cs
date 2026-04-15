using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Corsair.Abstractions;

namespace Corsair.Specs;

/// <summary>
/// Abstract base class for character-delimited flat file specifications
/// (CSV, TSV, pipe-separated, etc.).
/// </summary>
/// <remarks>
/// Subclass this to define a layout for a specific DTO type:
/// <code>
/// public class ProductSpec : DelimitedSpec&lt;Product&gt;
/// {
///     public ProductSpec()
///     {
///         Delimiter     = ',';
///         IncludeHeader = true;
///
///         Field(x => x.Sku,   header: "SKU");
///         Field(x => x.Name);
///         Field(x => x.Price, format: "F2");
///     }
/// }
/// </code>
/// </remarks>
/// <typeparam name="T">The DTO type whose instances this spec describes.</typeparam>
public abstract class DelimitedSpec<T> : IDelimitedSpec<T>
{
    private readonly List<IDelimitedFieldSpec<T>> _fields = new List<IDelimitedFieldSpec<T>>();

    /// <inheritdoc/>
    public char Delimiter { get; protected set; } = ',';

    /// <inheritdoc/>
    public bool IncludeHeader { get; protected set; } = true;

    /// <inheritdoc/>
    public IReadOnlyList<IDelimitedFieldSpec<T>> Fields => _fields.AsReadOnly();

    // -----------------------------------------------------------------------
    // Fluent builder
    // -----------------------------------------------------------------------

    /// <summary>
    /// Maps a DTO member to a delimited column and appends it to the field list.
    /// </summary>
    /// <param name="selector">
    /// A simple member-access expression that identifies the property or field to export,
    /// e.g. <c>x => x.Amount</c>.
    /// </param>
    /// <param name="header">
    /// The column name written in the header row. When <c>null</c> the member name
    /// derived from <paramref name="selector"/> is used instead.
    /// </param>
    /// <param name="format">
    /// An optional format string passed to <see cref="IFormattable.ToString(string,System.IFormatProvider)"/>
    /// when the value implements <see cref="IFormattable"/>; otherwise ignored.
    /// Example: <c>"yyyy-MM-dd"</c>, <c>"F4"</c>.
    /// </param>
    /// <returns>The current spec instance to allow method chaining.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="selector"/> is not a simple member-access expression.
    /// </exception>
    protected DelimitedSpec<T> Field(
        Expression<Func<T, object?>> selector,
        string? header = null,
        string? format = null)
    {
        _fields.Add(new DelimitedFieldSpec<T>(selector, header, format));
        return this;
    }
}
