namespace Switch.Iso8583.Exceptions;

/// <summary>Base type for every domain error raised by the ISO 8583 parser/builder.</summary>
public abstract class Iso8583Exception : Exception
{
    protected Iso8583Exception(string message) : base(message)
    {
    }
}
