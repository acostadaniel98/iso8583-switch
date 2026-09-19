namespace Switch.Iso8583.Tests;

/// <summary>
/// A field dictionary used only by tests, to exercise field types/numbers that
/// <see cref="IsoFieldDefinitions.Standard"/>'s realistic business subset never uses:
/// an LLLVAR field, and a field number above 64 (which requires a secondary bitmap).
/// See docs/adr/0003-diccionario-de-campos-inyectable.md for why this exists.
/// </summary>
internal static class TestFieldDefinitions
{
    public static readonly IReadOnlyDictionary<int, IsoFieldDefinition> All = new Dictionary<int, IsoFieldDefinition>
    {
        [3] = new IsoFieldDefinition(3, IsoFieldType.Fixed, IsoFieldFormat.Numeric, 6),
        [2] = new IsoFieldDefinition(2, IsoFieldType.Llvar, IsoFieldFormat.Numeric, 19),
        [43] = new IsoFieldDefinition(43, IsoFieldType.Lllvar, IsoFieldFormat.AlphaNumericSpecial, 40),
        [90] = new IsoFieldDefinition(90, IsoFieldType.Fixed, IsoFieldFormat.Numeric, 42)
    };
}
