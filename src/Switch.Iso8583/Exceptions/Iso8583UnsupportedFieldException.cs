namespace Switch.Iso8583.Exceptions;

/// <summary>
/// Raised when the bitmap flags a field number that has no entry in <see cref="IsoFieldDefinitions.Standard"/>,
/// so the parser has no rule to know how many bytes to consume for it.
/// </summary>
public sealed class Iso8583UnsupportedFieldException : Iso8583Exception
{
    public int FieldNumber { get; }

    public Iso8583UnsupportedFieldException(int fieldNumber)
        : base($"Field {fieldNumber} is present in the bitmap but has no known field definition.")
    {
        FieldNumber = fieldNumber;
    }
}
