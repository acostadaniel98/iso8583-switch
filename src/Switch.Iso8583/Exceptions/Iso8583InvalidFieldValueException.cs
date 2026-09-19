namespace Switch.Iso8583.Exceptions;

/// <summary>
/// Raised by <see cref="Iso8583MessageBuilder"/> when a field value does not fit its
/// definition — wrong format (e.g. non-digits in a numeric field) or too long for its
/// fixed/maximum length.
/// </summary>
public sealed class Iso8583InvalidFieldValueException : Iso8583Exception
{
    public int FieldNumber { get; }

    public Iso8583InvalidFieldValueException(int fieldNumber, string reason)
        : base($"Field {fieldNumber} has an invalid value: {reason}")
    {
        FieldNumber = fieldNumber;
    }
}
