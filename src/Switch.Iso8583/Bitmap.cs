using Switch.Iso8583.Exceptions;

namespace Switch.Iso8583;

/// <summary>
/// A primary bitmap (fields 1-64) plus an optional secondary bitmap (fields 65-128), each
/// packed as 8 bytes / 64 bits, MSB-first, per the ISO 8583 wire format. Bit 1 of the primary
/// bitmap is reserved: it is never a data field, it only flags whether a secondary bitmap
/// follows, and this class computes it automatically rather than accepting it as input.
/// </summary>
public sealed class Bitmap
{
    private const int BitsPerMap = 64;
    private const int BytesPerMap = 8;

    private readonly bool[] _primary = new bool[BitsPerMap];
    private readonly bool[] _secondary = new bool[BitsPerMap];

    private Bitmap()
    {
    }

    /// <summary>Whether any field 65-128 is present, i.e. whether a secondary bitmap is transmitted.</summary>
    public bool HasSecondary => Array.Exists(_secondary, isPresent => isPresent);

    /// <summary>Builds a bitmap flagging exactly the given data field numbers (2-128) as present.</summary>
    public static Bitmap FromFields(IEnumerable<int> fieldNumbers)
    {
        var bitmap = new Bitmap();
        foreach (var number in fieldNumbers)
        {
            if (number is < 2 or > 128)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(fieldNumbers), number, "Field numbers must be between 2 and 128; field 1 is reserved by the bitmap itself.");
            }

            if (number <= BitsPerMap)
            {
                bitmap._primary[number - 1] = true;
            }
            else
            {
                bitmap._secondary[number - BitsPerMap - 1] = true;
            }
        }

        return bitmap;
    }

    /// <summary>Reads a bitmap from the start of <paramref name="buffer"/>, returning how many bytes it consumed.</summary>
    public static Bitmap Parse(ReadOnlySpan<byte> buffer, out int bytesConsumed)
    {
        if (buffer.Length < BytesPerMap)
        {
            throw new Iso8583MalformedMessageException(
                $"Primary bitmap requires {BytesPerMap} bytes but only {buffer.Length} were available.");
        }

        var primaryBits = Unpack(buffer[..BytesPerMap]);
        var hasSecondary = primaryBits[0];

        var bitmap = new Bitmap();
        for (var i = 1; i < BitsPerMap; i++)
        {
            bitmap._primary[i] = primaryBits[i];
        }

        if (!hasSecondary)
        {
            bytesConsumed = BytesPerMap;
            return bitmap;
        }

        if (buffer.Length < BytesPerMap * 2)
        {
            throw new Iso8583MalformedMessageException(
                $"Bitmap flags a secondary bitmap but only {buffer.Length} bytes were available ({BytesPerMap * 2} required).");
        }

        var secondaryBits = Unpack(buffer.Slice(BytesPerMap, BytesPerMap));
        for (var i = 0; i < BitsPerMap; i++)
        {
            bitmap._secondary[i] = secondaryBits[i];
        }

        bytesConsumed = BytesPerMap * 2;
        return bitmap;
    }

    public bool IsSet(int fieldNumber)
    {
        ValidateFieldRange(fieldNumber);
        return fieldNumber <= BitsPerMap ? _primary[fieldNumber - 1] : _secondary[fieldNumber - BitsPerMap - 1];
    }

    /// <summary>Data field numbers present, in ascending order. Never includes field 1.</summary>
    public IEnumerable<int> PresentFields()
    {
        for (var i = 1; i < BitsPerMap; i++)
        {
            if (_primary[i])
            {
                yield return i + 1;
            }
        }

        for (var i = 0; i < BitsPerMap; i++)
        {
            if (_secondary[i])
            {
                yield return i + BitsPerMap + 1;
            }
        }
    }

    /// <summary>Serializes to 8 bytes, or 16 if a secondary bitmap is present.</summary>
    public byte[] ToBytes()
    {
        var primaryBits = (bool[])_primary.Clone();
        primaryBits[0] = HasSecondary;

        if (!HasSecondary)
        {
            return Pack(primaryBits);
        }

        var bytes = new byte[BytesPerMap * 2];
        Pack(primaryBits).CopyTo(bytes, 0);
        Pack(_secondary).CopyTo(bytes, BytesPerMap);
        return bytes;
    }

    private static void ValidateFieldRange(int fieldNumber)
    {
        if (fieldNumber is < 2 or > 128)
        {
            throw new ArgumentOutOfRangeException(nameof(fieldNumber), fieldNumber, "Field numbers must be between 2 and 128.");
        }
    }

    private static byte[] Pack(bool[] bits)
    {
        var bytes = new byte[BytesPerMap];
        for (var i = 0; i < bits.Length; i++)
        {
            if (bits[i])
            {
                bytes[i / 8] |= (byte)(0x80 >> (i % 8));
            }
        }

        return bytes;
    }

    private static bool[] Unpack(ReadOnlySpan<byte> eightBytes)
    {
        var bits = new bool[BitsPerMap];
        for (var i = 0; i < BitsPerMap; i++)
        {
            bits[i] = (eightBytes[i / 8] & (0x80 >> (i % 8))) != 0;
        }

        return bits;
    }
}
