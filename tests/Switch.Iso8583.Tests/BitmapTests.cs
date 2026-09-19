using Switch.Iso8583.Exceptions;

namespace Switch.Iso8583.Tests;

public class BitmapTests
{
    [Fact]
    public void FromFields_WithOnlyLowFields_HasNoSecondary()
    {
        var bitmap = Bitmap.FromFields(new[] { 2, 3, 4 });

        Assert.False(bitmap.HasSecondary);
        Assert.Equal(8, bitmap.ToBytes().Length);
    }

    [Fact]
    public void FromFields_WithFieldAbove64_HasSecondary()
    {
        var bitmap = Bitmap.FromFields(new[] { 2, 70 });

        Assert.True(bitmap.HasSecondary);
        Assert.Equal(16, bitmap.ToBytes().Length);
        Assert.True(bitmap.IsSet(70));
    }

    [Fact]
    public void ToBytes_FirstBit_ReflectsSecondaryPresence()
    {
        var withSecondary = Bitmap.FromFields(new[] { 70 });
        var withoutSecondary = Bitmap.FromFields(new[] { 2 });

        Assert.Equal(0x80, withSecondary.ToBytes()[0] & 0x80);
        Assert.Equal(0, withoutSecondary.ToBytes()[0] & 0x80);
    }

    [Fact]
    public void PresentFields_ReturnsAscendingOrderAcrossBothMaps()
    {
        var bitmap = Bitmap.FromFields(new[] { 100, 3, 65, 2 });

        Assert.Equal(new[] { 2, 3, 65, 100 }, bitmap.PresentFields());
    }

    [Fact]
    public void Parse_RoundTripsWithoutSecondary()
    {
        var original = Bitmap.FromFields(new[] { 2, 3, 49 });

        var parsed = Bitmap.Parse(original.ToBytes(), out var bytesConsumed);

        Assert.Equal(8, bytesConsumed);
        Assert.Equal(original.PresentFields(), parsed.PresentFields());
        Assert.False(parsed.HasSecondary);
    }

    [Fact]
    public void Parse_RoundTripsWithSecondary()
    {
        var original = Bitmap.FromFields(new[] { 2, 3, 100, 128 });

        var parsed = Bitmap.Parse(original.ToBytes(), out var bytesConsumed);

        Assert.Equal(16, bytesConsumed);
        Assert.Equal(original.PresentFields(), parsed.PresentFields());
        Assert.True(parsed.HasSecondary);
    }

    [Fact]
    public void Parse_BufferShorterThanPrimaryMap_ThrowsMalformed()
    {
        var tooShort = new byte[7];

        Assert.Throws<Iso8583MalformedMessageException>(() => Bitmap.Parse(tooShort, out _));
    }

    [Fact]
    public void Parse_FlagsSecondaryButBufferIsOnlyEightBytes_ThrowsMalformed()
    {
        var full = Bitmap.FromFields(new[] { 70 }).ToBytes();
        var truncated = full[..8];

        Assert.Throws<Iso8583MalformedMessageException>(() => Bitmap.Parse(truncated, out _));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(129)]
    public void FromFields_FieldNumberOutOfRange_Throws(int fieldNumber)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Bitmap.FromFields(new[] { fieldNumber }));
    }
}
