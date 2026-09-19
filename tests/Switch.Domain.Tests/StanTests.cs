namespace Switch.Domain.Tests;

public class StanTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(1000000)]
    public void Constructor_OutOfRange_Throws(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Stan(value));
    }

    [Theory]
    [InlineData(0, "000000")]
    [InlineData(42, "000042")]
    [InlineData(999999, "999999")]
    public void ToString_PadsToSixDigits(int value, string expected)
    {
        var stan = new Stan(value);

        Assert.Equal(expected, stan.ToString());
    }

    [Fact]
    public void Equals_SameValue_AreEqual()
    {
        Assert.Equal(new Stan(123456), new Stan(123456));
    }
}
