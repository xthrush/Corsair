namespace Corsair.Abstractions;

/// <summary>
/// Controls how a value is aligned within a fixed-width field.
/// </summary>
public enum FieldAlignment
{
    /// <summary>Value is left-aligned; padding is appended on the right.</summary>
    Left,

    /// <summary>Value is right-aligned; padding is prepended on the left.</summary>
    Right
}
