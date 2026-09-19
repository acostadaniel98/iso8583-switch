namespace Switch.Iso8583.Tests;

public class RoundTripTests
{
    [Fact]
    public void BuildThenParseThenSerialize_ReproducesOriginalBytesExactly_WithoutSecondaryBitmap()
    {
        var original = new Iso8583MessageBuilder()
            .WithMti("0200")
            .WithField(2, "4111111111111111")
            .WithField(3, "000000")
            .WithField(4, "000000012345")
            .WithField(7, "0919120000")
            .WithField(11, "123456")
            .WithField(12, "120000")
            .WithField(13, "0919")
            .WithField(14, "2812")
            .WithField(22, "051")
            .WithField(25, "00")
            .WithField(32, "12345")
            .WithField(37, "REF123456789")
            .WithField(41, "TERM0001")
            .WithField(42, "MERCHANT000001")
            .WithField(49, "840")
            .Build();

        var bytes1 = original.ToBytes();
        var parsed = Iso8583MessageParser.Parse(bytes1);
        var bytes2 = parsed.ToBytes();

        Assert.Equal(bytes1, bytes2);
        Assert.Equal(original.Mti, parsed.Mti);
        Assert.Equal(original.Fields.OrderBy(kv => kv.Key), parsed.Fields.OrderBy(kv => kv.Key));
    }

    [Fact]
    public void BuildThenParseThenSerialize_ReproducesOriginalBytesExactly_WithSecondaryBitmap()
    {
        var original = new Iso8583MessageBuilder(TestFieldDefinitions.All)
            .WithMti("0200")
            .WithField(2, "4111111111111111")
            .WithField(3, "000000")
            .WithField(43, "Some LLLVAR field content")
            .WithField(90, new string('9', 42))
            .Build();

        var bytes1 = original.ToBytes();
        var parsed = Iso8583MessageParser.Parse(bytes1, TestFieldDefinitions.All);
        var bytes2 = parsed.ToBytes();

        Assert.Equal(bytes1, bytes2);
        Assert.Equal(original.Fields.OrderBy(kv => kv.Key), parsed.Fields.OrderBy(kv => kv.Key));
    }

    [Fact]
    public void BuildThenParseThenSerialize_WithZeroLengthLlvarField_RoundTrips()
    {
        var original = new Iso8583MessageBuilder()
            .WithMti("0200")
            .WithField(32, "")
            .Build();

        var bytes1 = original.ToBytes();
        var parsed = Iso8583MessageParser.Parse(bytes1);

        Assert.Equal(bytes1, parsed.ToBytes());
        Assert.Equal("", parsed.GetField(32));
    }
}
