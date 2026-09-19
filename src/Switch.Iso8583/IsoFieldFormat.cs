namespace Switch.Iso8583;

/// <summary>Data format of a field's content, per the ISO 8583 field definitions table.</summary>
public enum IsoFieldFormat
{
    /// <summary>Numeric — digits only (0-9).</summary>
    Numeric,

    /// <summary>Alpha — letters and spaces only.</summary>
    Alpha,

    /// <summary>Alphanumeric special — printable ASCII.</summary>
    AlphaNumericSpecial,

    /// <summary>Binary — raw byte content.</summary>
    Binary
}
