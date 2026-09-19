using Switch.Iso8583.Exceptions;

namespace Switch.Iso8583.Tests;

public class Iso8583MessageBuilderTests
{
    [Fact]
    public void Build_WithoutMti_Throws()
    {
        var builder = new Iso8583MessageBuilder();

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Theory]
    [InlineData("020")]
    [InlineData("02000")]
    [InlineData("2A00")]
    public void WithMti_Invalid_Throws(string mti)
    {
        var builder = new Iso8583MessageBuilder();

        Assert.Throws<ArgumentException>(() => builder.WithMti(mti));
    }

    [Fact]
    public void WithField_UnknownFieldNumber_ThrowsUnsupported()
    {
        var builder = new Iso8583MessageBuilder();

        var exception = Assert.Throws<Iso8583UnsupportedFieldException>(() => builder.WithField(999, "x"));
        Assert.Equal(999, exception.FieldNumber);
    }

    [Fact]
    public void WithField_NumericFixed_PadsLeftWithZeros()
    {
        var message = new Iso8583MessageBuilder()
            .WithMti("0200")
            .WithField(11, "42")
            .Build();

        Assert.Equal("000042", message.GetField(11));
    }

    [Fact]
    public void WithField_AlphaFixed_PadsRightWithSpaces()
    {
        var message = new Iso8583MessageBuilder()
            .WithMti("0200")
            .WithField(38, "AB")
            .Build();

        Assert.Equal("AB    ", message.GetField(38));
    }

    [Fact]
    public void WithField_ValueLongerThanFixedLength_Throws()
    {
        var builder = new Iso8583MessageBuilder().WithMti("0200");

        var exception = Assert.Throws<Iso8583InvalidFieldValueException>(() => builder.WithField(11, "1234567"));
        Assert.Equal(11, exception.FieldNumber);
    }

    [Fact]
    public void WithField_NonDigitsInNumericField_Throws()
    {
        var builder = new Iso8583MessageBuilder().WithMti("0200");

        Assert.Throws<Iso8583InvalidFieldValueException>(() => builder.WithField(11, "12A456"));
    }

    [Fact]
    public void WithField_LlvarEmptyValue_IsAllowed()
    {
        var message = new Iso8583MessageBuilder()
            .WithMti("0200")
            .WithField(32, "")
            .Build();

        Assert.Equal("", message.GetField(32));
    }

    [Fact]
    public void WithField_LlvarValueLongerThanMax_Throws()
    {
        var builder = new Iso8583MessageBuilder().WithMti("0200");

        Assert.Throws<Iso8583InvalidFieldValueException>(() => builder.WithField(32, new string('1', 12)));
    }

    [Fact]
    public void WithField_Lllvar_UsesInjectedDictionary()
    {
        var message = new Iso8583MessageBuilder(TestFieldDefinitions.All)
            .WithMti("0200")
            .WithField(43, "some LLLVAR value")
            .Build();

        Assert.Equal("some LLLVAR value", message.GetField(43));
    }
}
