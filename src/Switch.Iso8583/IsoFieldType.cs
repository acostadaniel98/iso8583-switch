namespace Switch.Iso8583;

/// <summary>How a field's length is encoded on the wire.</summary>
public enum IsoFieldType
{
    /// <summary>Fixed-length field — no length prefix, exactly <c>Length</c> bytes.</summary>
    Fixed,

    /// <summary>Variable-length field with a 2-digit ASCII length prefix (max length 99).</summary>
    Llvar,

    /// <summary>Variable-length field with a 3-digit ASCII length prefix (max length 999).</summary>
    Lllvar
}
