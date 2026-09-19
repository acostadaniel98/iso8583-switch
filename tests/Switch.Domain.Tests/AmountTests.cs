namespace Switch.Domain.Tests;

public class AmountTests
{
    [Fact]
    public void Constructor_NegativeMinorUnits_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Amount(-1));
    }

    [Fact]
    public void Constructor_Zero_IsAllowed()
    {
        var amount = new Amount(0);

        Assert.Equal(0, amount.MinorUnits);
    }

    [Fact]
    public void ToDecimal_ConvertsMinorUnitsToMajorUnits()
    {
        var amount = new Amount(123456);

        Assert.Equal(1234.56m, amount.ToDecimal());
    }

    [Fact]
    public void ToString_FormatsWithTwoDecimals()
    {
        var amount = new Amount(500);

        Assert.Equal("5.00", amount.ToString());
    }

    [Fact]
    public void Equals_SameMinorUnits_AreEqual()
    {
        Assert.Equal(new Amount(1000), new Amount(1000));
    }
}
