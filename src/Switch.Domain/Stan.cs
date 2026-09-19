namespace Switch.Domain;

/// <summary>System Trace Audit Number: a 6-digit sequence number that ties a request to its response.</summary>
public sealed record Stan
{
    private const int MaxValue = 999999;

    public int Value { get; }

    public Stan(int value)
    {
        if (value < 0 || value > MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"STAN must be between 0 and {MaxValue}.");
        }

        Value = value;
    }

    public override string ToString() => Value.ToString("D6");
}
