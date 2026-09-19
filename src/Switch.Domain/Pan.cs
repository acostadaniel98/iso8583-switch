namespace Switch.Domain;

/// <summary>A Primary Account Number. Always carries its digits; only ever printed masked.</summary>
public sealed record Pan
{
    private const int MinLength = 12;
    private const int MaxLength = 19;

    public string Value { get; }

    public Pan(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length is < MinLength or > MaxLength || !value.All(char.IsAsciiDigit))
        {
            throw new ArgumentException(
                $"PAN must be {MinLength}-{MaxLength} digits, got '{value}'.", nameof(value));
        }

        Value = value;
    }

    /// <summary>PCI-DSS style masking for logs: first 6 and last 4 digits visible, the rest replaced with '*'.</summary>
    public string Masked()
    {
        var visibleStart = Math.Min(6, Value.Length);
        var visibleEnd = Math.Min(4, Value.Length - visibleStart);
        var maskedLength = Value.Length - visibleStart - visibleEnd;

        return Value[..visibleStart] + new string('*', maskedLength) + Value[^visibleEnd..];
    }

    public override string ToString() => Masked();
}
