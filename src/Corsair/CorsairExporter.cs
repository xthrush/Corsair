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
        var fields = spec.Fields; // resolve order once, not per record

        foreach (var record in records)
        {
            if (!first) sb.Append('\n');
            first = false;

            foreach (var field in fields)
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
        var fields = spec.Fields; // resolve order once, not per record

        foreach (var record in records)
        {
            if (!first) writer.Write('\n');
            first = false;

            foreach (var field in fields)
                writer.Write(RenderFixedField(field, record));
        }
    }

    // -----------------------------------------------------------------------
    // Delimited — string
    // -----------------------------------------------------------------------

    /// <inheritdoc/>
    public string Export<T>(IDelimitedSpec<T> spec, IEnumerable<T> records)
    {
        var sb = new StringBuilder();
        bool needsNewlineBefore = false;
        var fields = spec.Fields; // resolve order once, not per record

        if (spec.IncludeHeader)
        {
            bool firstField = true;
            foreach (var field in fields)
            {
                if (!firstField) sb.Append(spec.Delimiter);
                firstField = false;
                sb.Append(QuoteIfNeeded(field.Header, spec.Delimiter));
            }
            needsNewlineBefore = true;
        }

        foreach (var record in records)
        {
            if (needsNewlineBefore) sb.Append('\n');
            needsNewlineBefore = true;

            bool firstField = true;
            foreach (var field in fields)
            {
                if (!firstField) sb.Append(spec.Delimiter);
                firstField = false;
                sb.Append(RenderDelimitedField(field, record, spec.Delimiter));
            }
        }

        return sb.ToString();
    }

    // -----------------------------------------------------------------------
    // Delimited — stream
    // -----------------------------------------------------------------------

    /// <inheritdoc/>
    public void Write<T>(IDelimitedSpec<T> spec, IEnumerable<T> records, Stream stream, Encoding? encoding = null)
    {
        var enc = encoding ?? new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        using var writer = new StreamWriter(stream, enc, bufferSize: 4096, leaveOpen: true);

        bool needsNewlineBefore = false;
        var fields = spec.Fields; // resolve order once, not per record

        if (spec.IncludeHeader)
        {
            bool firstField = true;
            foreach (var field in fields)
            {
                if (!firstField) writer.Write(spec.Delimiter);
                firstField = false;
                writer.Write(QuoteIfNeeded(field.Header, spec.Delimiter));
            }
            needsNewlineBefore = true;
        }

        foreach (var record in records)
        {
            if (needsNewlineBefore) writer.Write('\n');
            needsNewlineBefore = true;

            bool firstField = true;
            foreach (var field in fields)
            {
                if (!firstField) writer.Write(spec.Delimiter);
                firstField = false;
                writer.Write(RenderDelimitedField(field, record, spec.Delimiter));
            }
        }
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static string RenderDelimitedField<T>(IDelimitedFieldSpec<T> field, T record, char delimiter)
    {
        var value = field.Getter(record);

        string text;
        if (value is null)
            text = string.Empty;
        else if (field.Format != null && value is IFormattable formattable)
            text = formattable.ToString(field.Format, CultureInfo.InvariantCulture);
        else
            text = value.ToString() ?? string.Empty;

        return QuoteIfNeeded(text, delimiter);
    }

    /// <summary>
    /// Wraps <paramref name="value"/> in double-quotes and escapes any internal
    /// double-quotes as <c>""</c> when the value contains the delimiter, a
    /// double-quote, or a line-break character. Returns the value unchanged otherwise.
    /// </summary>
    private static string QuoteIfNeeded(string value, char delimiter)
    {
        if (value.IndexOf(delimiter) < 0 &&
            value.IndexOf('"')       < 0 &&
            value.IndexOf('\n')      < 0 &&
            value.IndexOf('\r')      < 0)
            return value;

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

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
