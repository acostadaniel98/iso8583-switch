namespace Switch.Domain.Tests;

public class ResponseCodeTests
{
    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("000")]
    public void Constructor_WrongLength_Throws(string value)
    {
        Assert.Throws<ArgumentException>(() => new ResponseCode(value));
    }

    [Fact]
    public void Constructor_Null_Throws()
    {
        Assert.Throws<ArgumentException>(() => new ResponseCode(null!));
    }

    [Fact]
    public void Approved_HasValueZeroZero()
    {
        Assert.Equal("00", ResponseCode.Approved.Value);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var code = new ResponseCode("05");

        Assert.Equal("05", code.ToString());
    }
}
