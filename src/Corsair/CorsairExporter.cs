using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Corsair.Abstractions;

namespace Corsair;

/// <summary>
/// The main export engine. Accepts a spec and a sequence of records and
/// produces a flat file as a string or writes it to a stream.
/// </summary>
/// <remarks>
/// Implementation is pending. Both overloads currently throw
/// <see cref="NotImplementedException"/>.
/// </remarks>
public sealed class CorsairExporter : IExporter
{
    /// <inheritdoc/>
    public string Export<T>(IFixedSpec<T> spec, IEnumerable<T> records)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public string Export<T>(IDelimitedSpec<T> spec, IEnumerable<T> records)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public void Write<T>(IFixedSpec<T> spec, IEnumerable<T> records, Stream stream, Encoding? encoding = null)
        => throw new NotImplementedException();

    /// <inheritdoc/>
    public void Write<T>(IDelimitedSpec<T> spec, IEnumerable<T> records, Stream stream, Encoding? encoding = null)
        => throw new NotImplementedException();
}
