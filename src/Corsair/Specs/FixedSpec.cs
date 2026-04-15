using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Corsair.Abstractions;

namespace Corsair.Specs;

/// <summary>
/// Abstract base class for fixed-width flat file specifications.
/// </summary>
/// <remarks>
/// Subclass this to define a layout for a specific DTO type:
/// <code>
/// public class OrderSpec : FixedSpec&lt;Order&gt;
/// {
///     public OrderSpec()
///     {
///         Field(x => x.OrderId,   width: 10, alignment: FieldAlignment.Right, padding: '0');
///         Field(x => x.Reference, width: 20);
///         Field(x => x.Total,     width: 12, alignment: FieldAlignment.Right, format: "F2");
///     }
/// }
/// </code>
/// </remarks>
/// <typeparam name="T">The DTO type whose instances this spec describes.</typeparam>
public abstract class FixedSpec<T> : IFixedSpec<T>
{
    private readonly List<IFixedFieldSpec<T>> _fields = new List<IFixedFieldSpec<T>>();

    /// <inheritdoc/>
    public IReadOnlyList<IFixedFieldSpec<T>> Fields
    {
        get
        {
            // If any field has order = 0 (unset), honour declaration order for all.
            // Only sort when every field carries an explicit non-zero value.
            if (_fields.Any(f => f.Order == 0))
                return _fields.AsReadOnly();

            return _fields.OrderBy(f => f.Order).ToList();
        }
    }

    // -----------------------------------------------------------------------
    // Fluent builder
    // -----------------------------------------------------------------------

    /// <summary>
    /// Maps a DTO member to a fixed-width column and appends it to the field list.
    /// </summary>
    /// <param name="selector">
    /// A simple member-access expression that identifies the property or field to export,
    /// e.g. <c>x => x.FirstName</c>.
    /// </param>
    /// <param name="width">
    /// The exact number of characters the column occupies. Values shorter than
    /// <paramref name="width"/> are padded; values longer are truncated.
    /// </param>
    /// <param name="alignment">
    /// Whether the value is aligned to the left (padded on the right) or to the
    /// right (padded on the left). Defaults to <see cref="FieldAlignment.Left"/>.
    /// </param>
    /// <param name="padding">
    /// The character used to fill unused positions. Defaults to <c>' '</c> (space).
    /// </param>
    /// <param name="format">
    /// An optional format string passed to <see cref="IFormattable.ToString(string,System.IFormatProvider)"/>
    /// when the value implements <see cref="IFormattable"/>; otherwise ignored.
    /// Example: <c>"D8"</c>, <c>"yyyy-MM-dd"</c>, <c>"F2"</c>.
    /// </param>
    /// <param name="order">
    /// Explicit column position. Defaults to <c>0</c> (unset). Ordering is applied only
    /// when every field in the spec carries a non-zero value; if any field is <c>0</c>
    /// declaration order is used for all fields.
    /// </param>
    /// <returns>The current spec instance to allow method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="width"/> is less than or equal to zero.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="selector"/> is not a simple member-access expression.
    /// </exception>
    protected FixedSpec<T> Field(
        Expression<Func<T, object?>> selector,
        int width,
        FieldAlignment alignment = FieldAlignment.Left,
        char padding = ' ',
        string? format = null,
        int order = 0)
    {
        if (width <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Field width must be greater than zero.");

        _fields.Add(new FixedFieldSpec<T>(selector, width, alignment, padding, format, order));
        return this;
    }
}
