using System;
using System.Linq.Expressions;
using Corsair.Abstractions;

namespace Corsair.Specs;

/// <summary>
/// Internal concrete implementation of <see cref="IDelimitedFieldSpec{T}"/>.
/// Instances are created exclusively by <see cref="DelimitedSpec{T}.Field"/>.
/// </summary>
internal sealed class DelimitedFieldSpec<T> : IDelimitedFieldSpec<T>
{
    internal DelimitedFieldSpec(
        Expression<Func<T, object?>> selector,
        string? header,
        string? format,
        int order)
    {
        Name   = ExtractMemberName(selector);
        Header = header ?? Name;
        Format = format;
        Order  = order;

        var compiled = selector.Compile();
        Getter = record => compiled(record);
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public string Header { get; }

    /// <inheritdoc/>
    public string? Format { get; }

    /// <inheritdoc/>
    public Func<T, object?> Getter { get; }

    /// <inheritdoc/>
    public int Order { get; }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static string ExtractMemberName(Expression<Func<T, object?>> selector)
    {
        var body = selector.Body;

        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
            body = unary.Operand;

        if (body is MemberExpression member)
            return member.Member.Name;

        throw new ArgumentException(
            "The selector must be a simple member-access expression (e.g. x => x.Amount).",
            nameof(selector));
    }
}
