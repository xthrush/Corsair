using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using Corsair.Abstractions;
using Corsair.Specs;
using Xunit;

namespace Corsair.Tests;

public sealed class FixedExportTests
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
    // Spec helper — wraps FixedSpec<T> to expose the protected Field() method
    // as a public Add() call so test bodies can build specs inline.
    // -----------------------------------------------------------------------

    private sealed class RecSpec : FixedSpec<Rec>
    {
        public RecSpec Add(
            Expression<Func<Rec, object?>> selector,
            int width,
            FieldAlignment alignment = FieldAlignment.Left,
            char padding = ' ',
            string? format = null,
            int order = 0)
        {
            Field(selector, width, alignment, padding, format, order);
            return this;
        }
    }

    private static RecSpec Spec() => new RecSpec();

    // -----------------------------------------------------------------------
    // Alignment and padding
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_LeftAlignment_PadsWithSpacesOnRight()
    {
        var result = _exporter.Export(
            Spec().Add(x => x.Name, width: 8),
            new[] { new Rec { Name = "Alice" } });

        Assert.Equal("Alice   ", result);
    }

    [Fact]
    public void Export_RightAlignment_PadsWithSpacesOnLeft()
    {
        var result = _exporter.Export(
            Spec().Add(x => x.Name, width: 8, alignment: FieldAlignment.Right),
            new[] { new Rec { Name = "Alice" } });

        Assert.Equal("   Alice", result);
    }

    [Fact]
    public void Export_LeftAlignment_CustomPaddingChar_PadsOnRight()
    {
        var result = _exporter.Export(
            Spec().Add(x => x.Name, width: 8, padding: '-'),
            new[] { new Rec { Name = "Hi" } });

        Assert.Equal("Hi------", result);
    }

    [Fact]
    public void Export_RightAlignment_CustomPaddingChar_PadsOnLeft()
    {
        var result = _exporter.Export(
            Spec().Add(x => x.Age, width: 6, alignment: FieldAlignment.Right, padding: '0'),
            new[] { new Rec { Age = 42 } });

        Assert.Equal("000042", result);
    }

    // -----------------------------------------------------------------------
    // Truncation
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_ValueLongerThanWidth_TruncatesFromRight()
    {
        var result = _exporter.Export(
            Spec().Add(x => x.Name, width: 5),
            new[] { new Rec { Name = "Alexander" } });

        Assert.Equal("Alexa", result);
    }

    [Fact]
    public void Export_ValueExactlyAtWidth_NoPaddingOrTruncation()
    {
        var result = _exporter.Export(
            Spec().Add(x => x.Name, width: 5),
            new[] { new Rec { Name = "Hello" } });

        Assert.Equal("Hello", result);
    }

    // -----------------------------------------------------------------------
    // Null handling
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_NullValue_ProducesFullWidthSpaces()
    {
        var result = _exporter.Export(
            Spec().Add(x => x.Name, width: 6),
            new[] { new Rec { Name = null } });

        Assert.Equal("      ", result);   // 6 spaces
    }

    [Fact]
    public void Export_NullValue_RespectsAlignmentAndPaddingChar()
    {
        // Null still fills the column with the configured padding character.
        var result = _exporter.Export(
            Spec().Add(x => x.Name, width: 4, alignment: FieldAlignment.Right, padding: '*'),
            new[] { new Rec { Name = null } });

        Assert.Equal("****", result);
    }

    // -----------------------------------------------------------------------
    // Format strings
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_FormatString_AppliedToFormattableValue()
    {
        var result = _exporter.Export(
            Spec().Add(x => x.Amount, width: 10, alignment: FieldAlignment.Right, format: "F2"),
            new[] { new Rec { Amount = 1234.5m } });

        Assert.Equal("   1234.50", result);   // "1234.50" right-aligned in 10
    }

    [Fact]
    public void Export_FormatString_AppliedToIntegerValue()
    {
        var result = _exporter.Export(
            Spec().Add(x => x.Age, width: 8, alignment: FieldAlignment.Right, padding: '0', format: "D6"),
            new[] { new Rec { Age = 7 } });

        Assert.Equal("00000007", result);   // "D6" → "000007", then zero-padded to width 8
    }

    // -----------------------------------------------------------------------
    // Multiple fields
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_MultipleFields_ConcatenatedOnSingleLine()
    {
        var spec = Spec()
            .Add(x => x.Name, width: 6)                              // left  → "Bob   "
            .Add(x => x.Age,  width: 4, alignment: FieldAlignment.Right);  // right → "  30"

        var result = _exporter.Export(spec, new[] { new Rec { Name = "Bob", Age = 30 } });

        Assert.Equal("Bob     30", result);
    }

    // -----------------------------------------------------------------------
    // Multiple records
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_MultipleRecords_OneLinePerRecordSeparatedByNewline()
    {
        var spec = Spec().Add(x => x.Name, width: 5);
        var records = new[]
        {
            new Rec { Name = "Alice" },
            new Rec { Name = "Bob" },
            new Rec { Name = "Eve" },
        };

        var result = _exporter.Export(spec, records);
        var lines = result.Split('\n');

        Assert.Equal(3, lines.Length);
        Assert.Equal("Alice", lines[0]);
        Assert.Equal("Bob  ", lines[1]);
        Assert.Equal("Eve  ", lines[2]);
    }

    [Fact]
    public void Export_MultipleRecords_NoTrailingNewline()
    {
        var spec = Spec().Add(x => x.Name, width: 3);
        var records = new[] { new Rec { Name = "A" }, new Rec { Name = "B" } };

        var result = _exporter.Export(spec, records);

        Assert.False(result.EndsWith("\n"), "Export must not append a trailing newline.");
    }

    // -----------------------------------------------------------------------
    // Empty collection
    // -----------------------------------------------------------------------

    [Fact]
    public void Export_EmptyCollection_ReturnsEmptyString()
    {
        var result = _exporter.Export(
            Spec().Add(x => x.Name, width: 10),
            Array.Empty<Rec>());

        Assert.Equal(string.Empty, result);
    }

    // -----------------------------------------------------------------------
    // Write to stream
    // -----------------------------------------------------------------------

    [Fact]
    public void Write_SingleRecord_WritesCorrectBytesToStream()
    {
        var spec = Spec().Add(x => x.Name, width: 8);

        using var ms = new MemoryStream();
        _exporter.Write(spec, new[] { new Rec { Name = "Alice" } }, ms);

        var text = Encoding.UTF8.GetString(ms.ToArray());
        Assert.Equal("Alice   ", text);
    }

    [Fact]
    public void Write_MultipleRecords_LinesDelimitedByNewline()
    {
        var spec = Spec().Add(x => x.Name, width: 5);
        var records = new[] { new Rec { Name = "Alice" }, new Rec { Name = "Bob" } };

        using var ms = new MemoryStream();
        _exporter.Write(spec, records, ms);

        var text = Encoding.UTF8.GetString(ms.ToArray());
        var lines = text.Split('\n');

        Assert.Equal(2, lines.Length);
        Assert.Equal("Alice", lines[0]);
        Assert.Equal("Bob  ", lines[1]);
    }

    [Fact]
    public void Write_DoesNotCloseStream()
    {
        var spec = Spec().Add(x => x.Name, width: 5);

        using var ms = new MemoryStream();
        _exporter.Write(spec, new[] { new Rec { Name = "Test" } }, ms);

        // Stream must still be usable after Write returns.
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
            .Add(x => x.Name, width: 6,                                      order: 2)
            .Add(x => x.Age,  width: 4, alignment: FieldAlignment.Right, order: 1);

        var result = _exporter.Export(spec, new[] { new Rec { Name = "Alice", Age = 30 } });

        // Age  (order 1, w=4, right): "  30"
        // Name (order 2, w=6, left ): "Alice "
        Assert.Equal("  30Alice ", result);
    }

    [Fact]
    public void Export_AnyFieldOrderIsZero_FallsBackToInsertionOrder()
    {
        // Name has an explicit order but Age uses the default 0 → insertion order wins.
        var spec = Spec()
            .Add(x => x.Name, width: 6,                                      order: 2)
            .Add(x => x.Age,  width: 4, alignment: FieldAlignment.Right, order: 0);

        var result = _exporter.Export(spec, new[] { new Rec { Name = "Alice", Age = 30 } });

        // Insertion order: Name first, Age second.
        Assert.Equal("Alice   30", result);
    }

    [Fact]
    public void Export_AllOrderZero_UsesInsertionOrder()
    {
        var spec = Spec()
            .Add(x => x.Name, width: 6)
            .Add(x => x.Age,  width: 4, alignment: FieldAlignment.Right);

        var result = _exporter.Export(spec, new[] { new Rec { Name = "Alice", Age = 30 } });

        Assert.Equal("Alice   30", result);
    }

    [Fact]
    public void Export_ThreeFieldsExplicitOrder_OutputsInCorrectSequence()
    {
        var spec = Spec()
            .Add(x => x.Name,   width: 5,                                        order: 3)
            .Add(x => x.Amount, width: 7, alignment: FieldAlignment.Right, format: "F2", order: 1)
            .Add(x => x.Age,    width: 4, alignment: FieldAlignment.Right, order: 2);

        var result = _exporter.Export(spec,
            new[] { new Rec { Name = "Bob", Age = 7, Amount = 12.5m } });

        // Amount (order 1, w=7, right, F2): "  12.50"
        // Age    (order 2, w=4, right     ): "   7"
        // Name   (order 3, w=5, left      ): "Bob  "
        Assert.Equal("  12.50   7Bob  ", result);
    }
}
