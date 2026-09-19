using System.Text;
using Switch.Iso8583.Exceptions;

namespace Switch.Iso8583.Tests;

public class Iso8583MessageParserTests
{
    [Fact]
    public void Parse_MessageWithoutSecondaryBitmap_ReadsAllFields()
    {
        var raw = new Iso8583MessageBuilder()
            .WithMti("0200")
            .WithField(3, "000000")
            .WithField(4, "000000012345")
            .WithField(11, "123456")
            .WithField(39, "00")
            .Build()
            .ToBytes();

        var message = Iso8583MessageParser.Parse(raw);

        Assert.Equal("0200", message.Mti);
        Assert.Equal("000000", message.GetField(3));
        Assert.Equal("000000012345", message.GetField(4));
        Assert.Equal("123456", message.GetField(11));
        Assert.Equal("00", message.GetField(39));
        Assert.False(message.HasField(2));
    }

    [Fact]
    public void Parse_MessageWithSecondaryBitmap_ReadsFieldsFromBothMaps()
    {
        var raw = new Iso8583MessageBuilder(TestFieldDefinitions.All)
            .WithMti("0200")
            .WithField(3, "000000")
            .WithField(90, new string('1', 42))
            .Build()
            .ToBytes();

        var message = Iso8583MessageParser.Parse(raw, TestFieldDefinitions.All);

        Assert.Equal("000000", message.GetField(3));
        Assert.Equal(new string('1', 42), message.GetField(90));
    }

    [Fact]
    public void Parse_LlvarFieldWithZeroLength_ParsesEmptyString()
    {
        var raw = new Iso8583MessageBuilder()
            .WithMti("0200")
            .WithField(32, "")
            .Build()
            .ToBytes();

        var message = Iso8583MessageParser.Parse(raw);

        Assert.Equal("", message.GetField(32));
    }

    [Fact]
    public void Parse_LlvarFieldAtMaxLength_ParsesFullValue()
    {
        var value = new string('7', 19);
        var raw = new Iso8583MessageBuilder()
            .WithMti("0200")
            .WithField(2, value)
            .Build()
            .ToBytes();

        var message = Iso8583MessageParser.Parse(raw);

        Assert.Equal(value, message.GetField(2));
    }

    [Fact]
    public void Parse_LllvarField_ReadsThreeDigitPrefix()
    {
        const string value = "example lllvar payload";
        var raw = new Iso8583MessageBuilder(TestFieldDefinitions.All)
            .WithMti("0200")
            .WithField(43, value)
            .Build()
            .ToBytes();

        var message = Iso8583MessageParser.Parse(raw, TestFieldDefinitions.All);

        Assert.Equal(value, message.GetField(43));
    }

    [Fact]
    public void Parse_BufferShorterThanMti_ThrowsMalformed()
    {
        Assert.Throws<Iso8583MalformedMessageException>(() => Iso8583MessageParser.Parse([0x30, 0x32]));
    }

    [Fact]
    public void Parse_NonDigitMti_ThrowsMalformed()
    {
        var raw = Encoding.ASCII.GetBytes("02X0").Concat(Bitmap.FromFields(Array.Empty<int>()).ToBytes()).ToArray();

        Assert.Throws<Iso8583MalformedMessageException>(() => Iso8583MessageParser.Parse(raw));
    }

    [Fact]
    public void Parse_BitmapFlagsFieldWithNoDefinition_ThrowsUnsupportedField()
    {
        var raw = Encoding.ASCII.GetBytes("0200").Concat(Bitmap.FromFields(new[] { 6 }).ToBytes()).ToArray();

        var exception = Assert.Throws<Iso8583UnsupportedFieldException>(() => Iso8583MessageParser.Parse(raw));
        Assert.Equal(6, exception.FieldNumber);
    }

    [Fact]
    public void Parse_SecondaryBitmapFlagsFieldWithNoDefinition_ThrowsUnsupportedField()
    {
        // Uses the real Standard dictionary: proves the parser correctly walks past a
        // secondary bitmap (bit 1 set, 16 bytes total) even though none of the fields the
        // business subset defines ever sit above field 64.
        var raw = Encoding.ASCII.GetBytes("0200").Concat(Bitmap.FromFields(new[] { 70 }).ToBytes()).ToArray();

        var exception = Assert.Throws<Iso8583UnsupportedFieldException>(() => Iso8583MessageParser.Parse(raw));
        Assert.Equal(70, exception.FieldNumber);
    }

    [Fact]
    public void Parse_TruncatedBeforeFixedFieldDataEnds_ThrowsMalformed()
    {
        var full = new Iso8583MessageBuilder()
            .WithMti("0200")
            .WithField(4, "000000012345")
            .Build()
            .ToBytes();

        var truncated = full[..^3];

        var exception = Assert.Throws<Iso8583MalformedMessageException>(() => Iso8583MessageParser.Parse(truncated));
        Assert.Contains("Field 4", exception.Message);
    }

    [Fact]
    public void Parse_TruncatedBeforeLlvarLengthPrefixComplete_ThrowsMalformed()
    {
        var full = new Iso8583MessageBuilder()
            .WithMti("0200")
            .WithField(2, "4111111111111111")
            .Build()
            .ToBytes();

        var truncated = full[..13]; // MTI(4) + bitmap(8) + 1 of the 2 prefix digits

        var exception = Assert.Throws<Iso8583MalformedMessageException>(() => Iso8583MessageParser.Parse(truncated));
        Assert.Contains("Field 2", exception.Message);
    }

    [Fact]
    public void Parse_TruncatedBeforeLlvarDataComplete_ThrowsMalformed()
    {
        var full = new Iso8583MessageBuilder()
            .WithMti("0200")
            .WithField(2, "4111111111111111")
            .Build()
            .ToBytes();

        var truncated = full[..20]; // full 2-digit prefix, only 6 of the 17 PAN bytes

        var exception = Assert.Throws<Iso8583MalformedMessageException>(() => Iso8583MessageParser.Parse(truncated));
        Assert.Contains("Field 2", exception.Message);
    }
}
