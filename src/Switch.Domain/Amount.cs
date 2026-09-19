namespace Switch.Domain;

/// <summary>A transaction amount, stored as minor currency units (e.g. cents) to avoid floating-point drift.</summary>
public sealed record Amount
{
    public long MinorUnits { get; }

    public Amount(long minorUnits)
    {
        if (minorUnits < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(minorUnits), minorUnits, "Amount cannot be negative.");
        }

        MinorUnits = minorUnits;
    }

    public decimal ToDecimal() => MinorUnits / 100m;

    public override string ToString() => ToDecimal().ToString("F2");
}
