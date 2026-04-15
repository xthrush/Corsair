using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Corsair.Abstractions;

/// <summary>
/// Renders records using a provided spec, either returning the result as a
/// <see cref="string"/> or writing it directly to a <see cref="Stream"/>.
/// </summary>
public interface IExporter
{
    /// <summary>Exports <paramref name="records"/> to a fixed-width string.</summary>
    /// <typeparam name="T">The DTO type.</typeparam>
    /// <param name="spec">The fixed-width spec that controls field layout.</param>
    /// <param name="records">The sequence of records to export.</param>
    /// <returns>A string containing one fixed-width line per record.</returns>
    string Export<T>(IFixedSpec<T> spec, IEnumerable<T> records);

    /// <summary>Exports <paramref name="records"/> to a delimited string.</summary>
    /// <typeparam name="T">The DTO type.</typeparam>
    /// <param name="spec">The delimited spec that controls field layout.</param>
    /// <param name="records">The sequence of records to export.</param>
    /// <returns>A string containing one delimited line per record (plus an optional header).</returns>
    string Export<T>(IDelimitedSpec<T> spec, IEnumerable<T> records);

    /// <summary>Writes <paramref name="records"/> as fixed-width lines to <paramref name="stream"/>.</summary>
    /// <typeparam name="T">The DTO type.</typeparam>
    /// <param name="spec">The fixed-width spec that controls field layout.</param>
    /// <param name="records">The sequence of records to export.</param>
    /// <param name="stream">The target stream. Must be writable.</param>
    /// <param name="encoding">
    /// The text encoding to use. Defaults to <see cref="Encoding.UTF8"/> when <c>null</c>.
    /// </param>
    void Write<T>(IFixedSpec<T> spec, IEnumerable<T> records, Stream stream, Encoding? encoding = null);

    /// <summary>Writes <paramref name="records"/> as delimited lines to <paramref name="stream"/>.</summary>
    /// <typeparam name="T">The DTO type.</typeparam>
    /// <param name="spec">The delimited spec that controls field layout.</param>
    /// <param name="records">The sequence of records to export.</param>
    /// <param name="stream">The target stream. Must be writable.</param>
    /// <param name="encoding">
    /// The text encoding to use. Defaults to <see cref="Encoding.UTF8"/> when <c>null</c>.
    /// </param>
    void Write<T>(IDelimitedSpec<T> spec, IEnumerable<T> records, Stream stream, Encoding? encoding = null);
}
