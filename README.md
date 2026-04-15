# Corsair

**Spec-driven flat file export for .NET.**

Corsair exports strongly typed DTOs to fixed-width and delimited flat files using clean, fluent spec classes. No attributes on your DTOs — all layout is defined in separate spec classes, keeping your models pure POCOs.

---

## Features

- Fixed-width and delimited (CSV, TSV, pipe-separated, etc.) export
- Fluent spec builder API — familiar, readable, self-documenting
- Pure POCO DTOs — no decoration required
- Multiple specs per DTO — each spec defines a completely independent layout
- Optional explicit field ordering — or just use declaration order
- Header row support for delimited exports
- Export to `string` or write directly to a `Stream` with encoding control
- No dependencies beyond the .NET runtime

---

## Installation

```bash
dotnet add package Corsair
```

---

## Quick Start

### Define your DTO

```csharp
public class Employee
{
    public string FirstName { get; set; }
    public string LastName  { get; set; }
    public decimal PayRate  { get; set; }
    public DateTime HireDate { get; set; }
}
```

### Fixed-Width Export

Define a spec by subclassing `FixedSpec<T>`:

```csharp
public class EmployeeFixedSpec : FixedSpec<Employee>
{
    public EmployeeFixedSpec()
    {
        Field(x => x.LastName,  width: 20);
        Field(x => x.FirstName, width: 20);
        Field(x => x.PayRate,   width: 10, alignment: FieldAlignment.Right, format: "F2");
        Field(x => x.HireDate,  width: 10, format: "yyyy-MM-dd");
    }
}
```

Export:

```csharp
var exporter = new CorsairExporter();
string output = exporter.Export(new EmployeeFixedSpec(), employees);
```

Each record becomes one fixed-width line. Values are padded or truncated to the declared width.

### Delimited Export

Define a spec by subclassing `DelimitedSpec<T>`:

```csharp
public class EmployeeCsvSpec : DelimitedSpec<Employee>
{
    public EmployeeCsvSpec()
    {
        Delimiter(',');
        IncludeHeader();
        Field(x => x.FirstName, header: "First Name");
        Field(x => x.LastName,  header: "Last Name");
        Field(x => x.PayRate,   header: "Pay Rate", format: "F2");
        Field(x => x.HireDate,  header: "Hire Date", format: "yyyy-MM-dd");
    }
}
```

Export:

```csharp
string csv = exporter.Export(new EmployeeCsvSpec(), employees);
```

### Write to a Stream

```csharp
using var stream = File.OpenWrite("employees.txt");
exporter.Write(new EmployeeFixedSpec(), employees, stream);

// With explicit encoding
exporter.Write(new EmployeeCsvSpec(), employees, stream, Encoding.ASCII);
```

---

## Multiple Specs Per DTO

A single DTO can have as many specs as you need — each one defines a completely independent layout. This is useful when the same data must be exported to multiple downstream systems with different format requirements.

```csharp
// Same Employee DTO, three different layouts
var payrollOutput = exporter.Export(new PayrollFixedSpec(),   employees);
var hrFeedOutput  = exporter.Export(new HrDelimitedSpec(),    employees);
var bankFileOutput = exporter.Export(new BankFixedSpec(),     employees);
```

---

## Field Ordering

By default fields are exported in declaration order — the order you call `Field(...)` in your spec constructor.

To use explicit ordering, assign a non-zero `order` value to **every** field. If all fields carry a non-zero order, Corsair sorts by that value before exporting. If any field retains the default of `0`, declaration order is preserved for all fields.

```csharp
public class OrderedSpec : FixedSpec<Employee>
{
    public OrderedSpec()
    {
        Field(x => x.PayRate,   width: 10, order: 3);
        Field(x => x.LastName,  width: 20, order: 1);
        Field(x => x.FirstName, width: 20, order: 2);
        // Output order: LastName, FirstName, PayRate
    }
}
```

---

## Fixed-Width Field Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `width` | `int` | required | Exact character width of the field |
| `alignment` | `FieldAlignment` | `Left` | `Left` or `Right` alignment within the field |
| `padding` | `char` | `' '` | Character used to pad the field to full width |
| `format` | `string?` | `null` | Format string passed to `ToString()` or `string.Format()` |
| `order` | `int` | `0` | Explicit column position (see Field Ordering above) |

---

## Delimited Field Options

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `header` | `string?` | field name | Column header written when `IncludeHeader()` is set |
| `format` | `string?` | `null` | Format string passed to `ToString()` or `string.Format()` |
| `order` | `int` | `0` | Explicit column position (see Field Ordering above) |

---

## Why Not Attributes?

Attribute-based export ties your layout definition to your DTO. If you need multiple export formats from the same DTO — payroll, HR feeds, bank files — you end up with a cluttered model decorated for every downstream system.

Corsair keeps your DTOs clean and puts all layout decisions in focused, testable spec classes. One DTO, as many specs as you need, no decoration required.

---

## License

MIT