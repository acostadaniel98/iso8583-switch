namespace Switch.Domain.Tests;

public class PanTests
{
    [Fact]
    public void Constructor_ValidPan_SetsValue()
    {
        var pan = new Pan("4111111111111111");

        Assert.Equal("4111111111111111", pan.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("41111111111")] // 11 digits, below minimum
    [InlineData("411111111111111111111")] // 21 digits, above maximum
    [InlineData("411111111111111A")] // contains a letter
    public void Constructor_Invalid_Throws(string value)
    {
        Assert.Throws<ArgumentException>(() => new Pan(value));
    }

    [Fact]
    public void Constructor_Null_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Pan(null!));
    }

    [Fact]
    public void Masked_SixteenDigitPan_KeepsFirstSixAndLastFour()
    {
        var pan = new Pan("4111111111111111");

        Assert.Equal("411111******1111", pan.Masked());
    }

    [Fact]
    public void Masked_TwelveDigitPan_MasksMiddleTwoDigits()
    {
        var pan = new Pan("411111111111");

        Assert.Equal("411111**1111", pan.Masked());
    }

    [Fact]
    public void ToString_ReturnsMaskedValue()
    {
        var pan = new Pan("4111111111111111");

        Assert.Equal(pan.Masked(), pan.ToString());
    }

    [Fact]
    public void Equals_SamePanValue_AreEqual()
    {
        Assert.Equal(new Pan("4111111111111111"), new Pan("4111111111111111"));
    }
}
