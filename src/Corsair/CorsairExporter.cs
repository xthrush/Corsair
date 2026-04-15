using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Corsair.Abstractions;

namespace Corsair;

/// <summary>
/// The main export engine. Accepts a spec and a sequence of records and
/// produces a flat file as a string or writes it to a stream.
/// </summary>
public sealed class CorsairExporter : IExporter
{
    // -----------------------------------------------------------------------
    // Fixed-width — string
    // -----------------------------------------------------------------------

    /// <inheritdoc/>
    public string Export<T>(IFixedSpec<T> spec, IEnumerable<T> records)
    {
        var sb = new StringBuilder();
        bool first = true;

        foreach (var record in records)
        {
            if (!first) sb.Append('\n');
            first = false;

            foreach (var field in spec.Fields)
                sb.Append(RenderFixedField(field, record));
        }

        return sb.ToString();
    }

    // -----------------------------------------------------------------------
    // Fixed-width — stream
    // -----------------------------------------------------------------------

    /// <inheritdoc/>
    public void Write<T>(IFixedSpec<T> spec, IEnumerable<T> records, Stream stream, Encoding? encoding = null)
    {
        // leaveOpen: true — we must not close the caller's stream.
        var enc = encoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        using var writer = new StreamWriter(stream, enc, bufferSize: 4096, leaveOpen: true);

        bool first = true;
        foreach (var record in records)
        {
            if (!first) writer.Write('\n');
            first = false;

            foreach (var field in spec.Fields)
                writer.Write(RenderFixedField(field, record));
        }
    }

    // -----------------------------------------------------------------------
    // Delimited — not yet implemented
    // -----------------------------------------------------------------------

    /// <inheritdoc/>
    public string Export<T>(IDelimitedSpec<T> spec, IEnumerable<T> records)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public void Write<T>(IDelimitedSpec<T> spec, IEnumerable<T> records, Stream stream, Encoding? encoding = null)
        => throw new NotImplementedException();

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static string RenderFixedField<T>(IFixedFieldSpec<T> field, T record)
    {
        var value = field.Getter(record);

        string text;
        if (value is null)
            text = string.Empty;
        else if (field.Format != null && value is IFormattable formattable)
            text = formattable.ToString(field.Format, CultureInfo.InvariantCulture);
        else
            text = value.ToString() ?? string.Empty;

        if (text.Length > field.Width)
            return text.Substring(0, field.Width);

        return field.Alignment == FieldAlignment.Left
            ? text.PadRight(field.Width, field.Padding)
            : text.PadLeft(field.Width, field.Padding);
    }
}
