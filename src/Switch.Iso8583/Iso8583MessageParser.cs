using System.Text;
using Switch.Iso8583.Exceptions;

namespace Switch.Iso8583;

/// <summary>
/// Parses a raw ISO 8583 wire message (MTI + bitmap(s) + fields) into a typed
/// <see cref="Iso8583Message"/>. Any inconsistency between what the bitmap or a length
/// prefix declares and what the buffer actually contains raises
/// <see cref="Iso8583MalformedMessageException"/> instead of a raw indexing exception.
/// </summary>
public static class Iso8583MessageParser
{
    private const int MtiLength = 4;

    /// <param name="fieldDefinitions">
    /// Field dictionary to interpret the bitmap against. Defaults to <see cref="IsoFieldDefinitions.Standard"/>;
    /// tests pass a custom dictionary to exercise field types (e.g. LLLVAR) or field numbers the standard
    /// subset does not happen to use.
    /// </param>
    public static Iso8583Message Parse(byte[] raw, IReadOnlyDictionary<int, IsoFieldDefinition>? fieldDefinitions = null)
    {
        ArgumentNullException.ThrowIfNull(raw);
        fieldDefinitions ??= IsoFieldDefinitions.Standard;

        ReadOnlySpan<byte> buffer = raw;

        if (buffer.Length < MtiLength)
        {
            throw new Iso8583MalformedMessageException(
                $"Message must start with a {MtiLength}-digit MTI but only {buffer.Length} bytes were available.");
        }

        var mti = DecodeAscii(buffer[..MtiLength]);
        if (!mti.All(char.IsAsciiDigit))
        {
            throw new Iso8583MalformedMessageException($"MTI must be {MtiLength} digits, got '{mti}'.");
        }

        var offset = MtiLength;
        var bitmap = Bitmap.Parse(buffer[offset..], out var bitmapBytesConsumed);
        offset += bitmapBytesConsumed;

        var fields = new Dictionary<int, string>();
        foreach (var fieldNumber in bitmap.PresentFields())
        {
            if (!fieldDefinitions.TryGetValue(fieldNumber, out var definition))
            {
                throw new Iso8583UnsupportedFieldException(fieldNumber);
            }

            var (value, bytesConsumed) = ReadField(buffer, offset, definition);
            fields[fieldNumber] = value;
            offset += bytesConsumed;
        }

        return new Iso8583Message(mti, fields, fieldDefinitions);
    }

    private static (string Value, int BytesConsumed) ReadField(ReadOnlySpan<byte> buffer, int offset, IsoFieldDefinition definition)
    {
        var length = definition.Length;
        var prefixDigits = 0;

        if (definition.Type != IsoFieldType.Fixed)
        {
            prefixDigits = definition.LengthPrefixDigits;
            if (offset + prefixDigits > buffer.Length)
            {
                throw new Iso8583MalformedMessageException(
                    $"Field {definition.Number}: expected a {prefixDigits}-digit length prefix but the buffer ended after {buffer.Length - offset} bytes.");
            }

            var prefix = DecodeAscii(buffer.Slice(offset, prefixDigits));
            if (!prefix.All(char.IsAsciiDigit) || !int.TryParse(prefix, out length))
            {
                throw new Iso8583MalformedMessageException($"Field {definition.Number}: length prefix '{prefix}' is not numeric.");
            }

            if (length > definition.Length)
            {
                throw new Iso8583MalformedMessageException(
                    $"Field {definition.Number}: declared length {length} exceeds the maximum of {definition.Length}.");
            }

            offset += prefixDigits;
        }

        if (offset + length > buffer.Length)
        {
            throw new Iso8583MalformedMessageException(
                $"Field {definition.Number}: expected {length} bytes but only {buffer.Length - offset} were available.");
        }

        var value = DecodeAscii(buffer.Slice(offset, length));
        return (value, prefixDigits + length);
    }

    private static string DecodeAscii(ReadOnlySpan<byte> bytes) => Encoding.ASCII.GetString(bytes);
}
