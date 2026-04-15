using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using Corsair.Abstractions;
using Corsair.Specs;
using Xunit;

namespace Corsair.Tests;

public sealed class DelimitedExportTests
{
    private readonly CorsairExporter _exporter = new CorsairExporter();

    // -----------------------------------------------------------------------
    // DTO used across all tests
    // -----------------------------------------------------------------------

    private sealed class Rec
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public decimal Amount { get; set; }
    }

    // -----------------------------------------------------------------------
    // Spec helper — exposes protected members as public fluent methods so
    // test bodies can build specs inline without subclassing per test.
    // -----------------------------------------------------------------------

    private sealed class RecSpec : DelimitedSpec<Rec>
    {
        public RecSpec Add(
            Expression<Func<Rec, object?>> selector,
            string? header = null,
            string? format = null,
            int order = 0)
        {
            Field(selector, header, format, order);
            return this;
        }

        public RecSpec WithDelimiter(char delimiter) { Delimiter = delimiter; return this; }
        public RecSpec WithoutHeader() { IncludeHeader = false; return this; }
    }

    private static RecSpec Spec() => new RecSpec();

    // -----------------------------------------------------------------------
    // Header row
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_WithHeader_WritesHeaderAsFirstLine()
    {
        var spec = Spec().Add(x => x.Name).Add(x => x.Age);
        var result = _exporter.Export(spec, new[] { new Rec { Name = "Alice", Age = 30 } });

        var lines = result.Split('\n');
        Assert.Equal(2, lines.Length);
        Assert.Equal("Name,Age", lines[0]);
    }

    [Fact]
    public void Export_WithoutHeader_SkipsHeaderRow()
    {
        var spec = Spec().WithoutHeader().Add(x => x.Name).Add(x => x.Age);
        var result = _exporter.Export(spec, new[] { new Rec { Name = "Alice", Age = 30 } });

        Assert.Equal("Alice,30", result);
    }

    [Fact]
    public void Export_Header_DefaultsToPropertyName()
    {
        var spec = Spec().Add(x => x.Name).Add(x => x.Age);

        // Empty records so the output is just the header line.
        var result = _exporter.Export(spec, Array.Empty<Rec>());

        Assert.Equal("Name,Age", result);
    }

    [Fact]
    public void Export_Header_UsesExplicitName()
    {
        var spec = Spec().Add(x => x.Name, header: "Full Name").Add(x => x.Age, header: "Years");

        var result = _exporter.Export(spec, Array.Empty<Rec>());

        Assert.Equal("Full Name,Years", result);
    }

    // -----------------------------------------------------------------------
    // Data rows
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_SingleRecord_DelimitsFieldsWithComma()
    {
        var spec = Spec().WithoutHeader().Add(x => x.Name).Add(x => x.Age);
        var result = _exporter.Export(spec, new[] { new Rec { Name = "Alice", Age = 30 } });

        Assert.Equal("Alice,30", result);
    }

    [Fact]
    public void Export_CustomDelimiter_UsesSpecifiedChar()
    {
        var spec = Spec().WithoutHeader().WithDelimiter('|').Add(x => x.Name).Add(x => x.Age);
        var result = _exporter.Export(spec, new[] { new Rec { Name = "Alice", Age = 30 } });

        Assert.Equal("Alice|30", result);
    }

    [Fact]
    public void Export_FormatString_AppliedToFormattableValue()
    {
        var spec = Spec().WithoutHeader().Add(x => x.Amount, format: "F2");
        var result = _exporter.Export(spec, new[] { new Rec { Amount = 1234.5m } });

        Assert.Equal("1234.50", result);
    }

    [Fact]
    public void Export_NullValue_ProducesEmptyField()
    {
        var spec = Spec().WithoutHeader().Add(x => x.Name).Add(x => x.Age);
        var result = _exporter.Export(spec, new[] { new Rec { Name = null, Age = 5 } });

        Assert.Equal(",5", result);
    }

    // -----------------------------------------------------------------------
    // Multiple records
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_MultipleRecords_OneLinePerRecord()
    {
        var spec = Spec().WithoutHeader().Add(x => x.Name).Add(x => x.Age);
        var records = new[]
        {
            new Rec { Name = "Alice", Age = 30 },
            new Rec { Name = "Bob",   Age = 25 },
        };

        var result = _exporter.Export(spec, records);
        var lines = result.Split('\n');

        Assert.Equal(2, lines.Length);
        Assert.Equal("Alice,30", lines[0]);
        Assert.Equal("Bob,25",   lines[1]);
    }

    [Fact]
    public void Export_MultipleRecords_NoTrailingNewline()
    {
        var spec = Spec().WithoutHeader().Add(x => x.Name);
        var records = new[] { new Rec { Name = "Alice" }, new Rec { Name = "Bob" } };

        var result = _exporter.Export(spec, records);

        Assert.False(result.EndsWith("\n"), "Export must not append a trailing newline.");
    }

    [Fact]
    public void Export_WithHeaderAndMultipleRecords_HeaderIsFirstLine()
    {
        var spec = Spec().Add(x => x.Name).Add(x => x.Age);
        var records = new[] { new Rec { Name = "Alice", Age = 30 }, new Rec { Name = "Bob", Age = 25 } };

        var result = _exporter.Export(spec, records);
        var lines = result.Split('\n');

        Assert.Equal(3, lines.Length);
        Assert.Equal("Name,Age", lines[0]);
        Assert.Equal("Alice,30", lines[1]);
        Assert.Equal("Bob,25",   lines[2]);
    }

    // -----------------------------------------------------------------------
    // Empty collection
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_EmptyCollection_WithoutHeader_ReturnsEmptyString()
    {
        var spec = Spec().WithoutHeader().Add(x => x.Name);
        var result = _exporter.Export(spec, Array.Empty<Rec>());

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Export_EmptyCollection_WithHeader_ReturnsOnlyHeaderLine()
    {
        var spec = Spec().Add(x => x.Name).Add(x => x.Age);
        var result = _exporter.Export(spec, Array.Empty<Rec>());

        // Header only — no trailing newline.
        Assert.Equal("Name,Age", result);
    }

    // -----------------------------------------------------------------------
    // RFC 4180 quoting
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_ValueContainsDelimiter_QuotesTheField()
    {
        var spec = Spec().WithoutHeader().Add(x => x.Name);
        var result = _exporter.Export(spec, new[] { new Rec { Name = "Smith, John" } });

        Assert.Equal("\"Smith, John\"", result);
    }

    [Fact]
    public void Export_ValueContainsDoubleQuote_EscapesAndQuotesField()
    {
        var spec = Spec().WithoutHeader().Add(x => x.Name);
        var result = _exporter.Export(spec, new[] { new Rec { Name = "He said \"Hello\"" } });

        // He said "Hello"  →  "He said ""Hello"""
        Assert.Equal("\"He said \"\"Hello\"\"\"", result);
    }

    [Fact]
    public void Export_HeaderContainsDelimiter_QuotesTheHeader()
    {
        // Use pipe delimiter and put a pipe in the header.
        var spec = Spec().WithDelimiter('|').Add(x => x.Name, header: "First|Last");
        var result = _exporter.Export(spec, Array.Empty<Rec>());

        Assert.Equal("\"First|Last\"", result);
    }

    [Fact]
    public void Export_ValueWithoutSpecialChars_NotQuoted()
    {
        var spec = Spec().WithoutHeader().Add(x => x.Name);
        var result = _exporter.Export(spec, new[] { new Rec { Name = "Alice" } });

        Assert.Equal("Alice", result);
    }

    // -----------------------------------------------------------------------
    // Write to stream
    // -----------------------------------------------------------------------

    [Fact]
    public void Write_WithHeader_MatchesExportOutput()
    {
        var spec = Spec().Add(x => x.Name).Add(x => x.Age);
        var records = new[] { new Rec { Name = "Alice", Age = 30 } };

        using var ms = new MemoryStream();
        _exporter.Write(spec, records, ms);

        var text = Encoding.UTF8.GetString(ms.ToArray());
        Assert.Equal("Name,Age\nAlice,30", text);
    }

    [Fact]
    public void Write_CustomDelimiter_WritesCorrectSeparator()
    {
        var spec = Spec().WithoutHeader().WithDelimiter('\t').Add(x => x.Name).Add(x => x.Age);
        var records = new[] { new Rec { Name = "Alice", Age = 30 } };

        using var ms = new MemoryStream();
        _exporter.Write(spec, records, ms);

        var text = Encoding.UTF8.GetString(ms.ToArray());
        Assert.Equal("Alice\t30", text);
    }

    [Fact]
    public void Write_DoesNotCloseStream()
    {
        var spec = Spec().Add(x => x.Name);

        using var ms = new MemoryStream();
        _exporter.Write(spec, new[] { new Rec { Name = "Alice" } }, ms);

        Assert.True(ms.CanRead);
        Assert.True(ms.CanWrite);
    }

    // -----------------------------------------------------------------------
    // Explicit field ordering
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_AllFieldsHaveExplicitOrder_OutputsSortedByOrderValue()
    {
        // Declared Name-first, Age-second, but order values reverse them.
        var spec = Spec()
            .WithoutHeader()
            .Add(x => x.Name, order: 2)
            .Add(x => x.Age,  order: 1);

        var result = _exporter.Export(spec, new[] { new Rec { Name = "Alice", Age = 30 } });

        Assert.Equal("30,Alice", result);
    }

    [Fact]
    public void Export_AnyFieldOrderIsZero_FallsBackToInsertionOrder()
    {
        // Name has an explicit order but Age uses the default 0 → insertion order wins.
        var spec = Spec()
            .WithoutHeader()
            .Add(x => x.Name, order: 2)
            .Add(x => x.Age,  order: 0);

        var result = _exporter.Export(spec, new[] { new Rec { Name = "Alice", Age = 30 } });

        Assert.Equal("Alice,30", result);
    }

    [Fact]
    public void Export_AllOrderZero_UsesInsertionOrder()
    {
        var spec = Spec().WithoutHeader().Add(x => x.Name).Add(x => x.Age);

        var result = _exporter.Export(spec, new[] { new Rec { Name = "Alice", Age = 30 } });

        Assert.Equal("Alice,30", result);
    }

    [Fact]
    public void Export_ExplicitOrder_HeaderRowReflectsReorderedColumns()
    {
        // Order also applies to the header row, not just data rows.
        var spec = Spec()
            .Add(x => x.Name, header: "Name", order: 2)
            .Add(x => x.Age,  header: "Age",  order: 1);

        var result = _exporter.Export(spec, Array.Empty<Rec>());

        Assert.Equal("Age,Name", result);
    }

    [Fact]
    public void Export_ThreeFieldsExplicitOrder_OutputsInCorrectSequence()
    {
        var spec = Spec()
            .WithoutHeader()
            .Add(x => x.Name,   order: 3)
            .Add(x => x.Amount, format: "F2", order: 1)
            .Add(x => x.Age,    order: 2);

        var result = _exporter.Export(spec,
            new[] { new Rec { Name = "Bob", Age = 7, Amount = 12.5m } });

        Assert.Equal("12.50,7,Bob", result);
    }
}
