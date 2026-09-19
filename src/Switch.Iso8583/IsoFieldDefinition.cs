namespace Switch.Iso8583;

/// <summary>
/// Describes how a single bitmap field (2-128) is encoded: its wire type, content format and
/// length — exact length for <see cref="IsoFieldType.Fixed"/>, maximum length for the LLVAR/LLLVAR types.
/// </summary>
public sealed record IsoFieldDefinition(int Number, IsoFieldType Type, IsoFieldFormat Format, int Length)
{
    /// <summary>Digits in the length prefix on the wire (0 for fixed fields).</summary>
    public int LengthPrefixDigits => Type switch
    {
        IsoFieldType.Fixed => 0,
        IsoFieldType.Llvar => 2,
        IsoFieldType.Lllvar => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(Type))
    };
}
