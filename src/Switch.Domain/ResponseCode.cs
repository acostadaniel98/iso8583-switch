namespace Switch.Domain;

/// <summary>The 2-character authorization response code (ISO 8583 field 39, e.g. "00" = approved).</summary>
public sealed record ResponseCode
{
    public static readonly ResponseCode Approved = new("00");

    public string Value { get; }

    public ResponseCode(string value)
    {
        if (value is null || value.Length != 2)
        {
            throw new ArgumentException($"Response code must be exactly 2 characters, got '{value}'.", nameof(value));
        }

        Value = value;
    }

    public override string ToString() => Value;
}
