namespace Switch.Iso8583.Exceptions;

/// <summary>
/// Raised when a raw message cannot be parsed as declared: the bitmap (or a length prefix)
/// claims more bytes than the buffer actually contains. Thrown instead of letting a raw
/// <see cref="IndexOutOfRangeException"/> or <see cref="ArgumentOutOfRangeException"/> leak out.
/// </summary>
public sealed class Iso8583MalformedMessageException : Iso8583Exception
{
    public Iso8583MalformedMessageException(string message) : base(message)
    {
    }
}
