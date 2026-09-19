using System.Text;

namespace Switch.Iso8583;

/// <summary>
/// A typed, already-validated ISO 8583 message: an MTI plus a set of data fields (2-128).
/// Instances are only produced by <see cref="Iso8583MessageBuilder"/> or <see cref="Iso8583MessageParser"/>,
/// so by the time application code sees one it is guaranteed to serialize back to a well-formed wire message.
/// </summary>
public sealed class Iso8583Message
{
    public string Mti { get; }

    public IReadOnlyDictionary<int, string> Fields { get; }

    private readonly IReadOnlyDictionary<int, IsoFieldDefinition> _fieldDefinitions;

    internal Iso8583Message(string mti, IReadOnlyDictionary<int, string> fields, IReadOnlyDictionary<int, IsoFieldDefinition> fieldDefinitions)
    {
        Mti = mti;
        Fields = fields;
        _fieldDefinitions = fieldDefinitions;
    }

    public bool HasField(int number) => Fields.ContainsKey(number);

    public string GetField(int number) => Fields.TryGetValue(number, out var value)
        ? value
        : throw new KeyNotFoundException($"Field {number} is not present in this message.");

    /// <summary>Serializes this message back to its raw wire representation: MTI + bitmap(s) + fields in ascending order.</summary>
    public byte[] ToBytes()
    {
        var bitmap = Bitmap.FromFields(Fields.Keys);

        using var stream = new MemoryStream();
        stream.Write(Encoding.ASCII.GetBytes(Mti));
        stream.Write(bitmap.ToBytes());

        foreach (var fieldNumber in bitmap.PresentFields())
        {
            var definition = _fieldDefinitions[fieldNumber];
            var value = Fields[fieldNumber];

            if (definition.Type != IsoFieldType.Fixed)
            {
                var lengthPrefix = value.Length.ToString().PadLeft(definition.LengthPrefixDigits, '0');
                stream.Write(Encoding.ASCII.GetBytes(lengthPrefix));
            }

            stream.Write(Encoding.ASCII.GetBytes(value));
        }

        return stream.ToArray();
    }
}
