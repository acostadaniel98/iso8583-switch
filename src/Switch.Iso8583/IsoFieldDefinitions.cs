namespace Switch.Iso8583;

/// <summary>
/// The subset of the ISO 8583 field dictionary supported by this switch: the fields
/// needed to model a realistic authorization request/response (0200/0210) pair.
/// Field 1 is deliberately absent — it is reserved by the bitmap itself as the
/// secondary-bitmap-present indicator and is never a data field.
/// </summary>
public static class IsoFieldDefinitions
{
    public static readonly IReadOnlyDictionary<int, IsoFieldDefinition> Standard =
        new Dictionary<int, IsoFieldDefinition>
        {
            [2] = new IsoFieldDefinition(2, IsoFieldType.Llvar, IsoFieldFormat.Numeric, 19),
            [3] = new IsoFieldDefinition(3, IsoFieldType.Fixed, IsoFieldFormat.Numeric, 6),
            [4] = new IsoFieldDefinition(4, IsoFieldType.Fixed, IsoFieldFormat.Numeric, 12),
            [7] = new IsoFieldDefinition(7, IsoFieldType.Fixed, IsoFieldFormat.Numeric, 10),
            [11] = new IsoFieldDefinition(11, IsoFieldType.Fixed, IsoFieldFormat.Numeric, 6),
            [12] = new IsoFieldDefinition(12, IsoFieldType.Fixed, IsoFieldFormat.Numeric, 6),
            [13] = new IsoFieldDefinition(13, IsoFieldType.Fixed, IsoFieldFormat.Numeric, 4),
            [14] = new IsoFieldDefinition(14, IsoFieldType.Fixed, IsoFieldFormat.Numeric, 4),
            [22] = new IsoFieldDefinition(22, IsoFieldType.Fixed, IsoFieldFormat.Numeric, 3),
            [25] = new IsoFieldDefinition(25, IsoFieldType.Fixed, IsoFieldFormat.Numeric, 2),
            [32] = new IsoFieldDefinition(32, IsoFieldType.Llvar, IsoFieldFormat.Numeric, 11),
            [37] = new IsoFieldDefinition(37, IsoFieldType.Fixed, IsoFieldFormat.Alpha, 12),
            [38] = new IsoFieldDefinition(38, IsoFieldType.Fixed, IsoFieldFormat.Alpha, 6),
            [39] = new IsoFieldDefinition(39, IsoFieldType.Fixed, IsoFieldFormat.Alpha, 2),
            [41] = new IsoFieldDefinition(41, IsoFieldType.Fixed, IsoFieldFormat.AlphaNumericSpecial, 8),
            [42] = new IsoFieldDefinition(42, IsoFieldType.Fixed, IsoFieldFormat.AlphaNumericSpecial, 15),
            [49] = new IsoFieldDefinition(49, IsoFieldType.Fixed, IsoFieldFormat.Numeric, 3)
        };
}
