using Switch.Iso8583.Exceptions;

namespace Switch.Iso8583;

/// <summary>
/// Fluent builder for an <see cref="Iso8583Message"/>: set the MTI, add fields one by one,
/// then <see cref="Build"/>. Numeric fixed fields are zero-padded on the left, alpha/ans
/// fixed fields are space-padded on the right, and variable-length fields are left as given.
/// </summary>
public sealed class Iso8583MessageBuilder
{
    private readonly IReadOnlyDictionary<int, IsoFieldDefinition> _fieldDefinitions;
    private string? _mti;
    private readonly Dictionary<int, string> _fields = new();

    /// <param name="fieldDefinitions">
    /// Field dictionary to validate and format against. Defaults to <see cref="IsoFieldDefinitions.Standard"/>;
    /// tests pass a custom dictionary to exercise field types (e.g. LLLVAR) or field numbers the standard
    /// subset does not happen to use.
    /// </param>
    public Iso8583MessageBuilder(IReadOnlyDictionary<int, IsoFieldDefinition>? fieldDefinitions = null)
    {
        _fieldDefinitions = fieldDefinitions ?? IsoFieldDefinitions.Standard;
    }

    public Iso8583MessageBuilder WithMti(string mti)
    {
        if (mti is null || mti.Length != 4 || !mti.All(char.IsAsciiDigit))
        {
            throw new ArgumentException($"MTI must be exactly 4 digits, got '{mti}'.", nameof(mti));
        }

        _mti = mti;
        return this;
    }

    public Iso8583MessageBuilder WithField(int number, string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (!_fieldDefinitions.TryGetValue(number, out var definition))
        {
            throw new Iso8583UnsupportedFieldException(number);
        }

        _fields[number] = definition.Type == IsoFieldType.Fixed
            ? FormatFixed(definition, value)
            : FormatVariable(definition, value);

        return this;
    }

    public Iso8583Message Build()
    {
        if (_mti is null)
        {
            throw new InvalidOperationException("Cannot build a message without an MTI. Call WithMti first.");
        }

        return new Iso8583Message(_mti, new Dictionary<int, string>(_fields), _fieldDefinitions);
    }

    private static string FormatFixed(IsoFieldDefinition definition, string value)
    {
        if (value.Length > definition.Length)
        {
            throw new Iso8583InvalidFieldValueException(
                definition.Number, $"value '{value}' is longer than the fixed length {definition.Length}.");
        }

        ValidateFormat(definition, value);

        return definition.Format == IsoFieldFormat.Numeric
            ? value.PadLeft(definition.Length, '0')
            : value.PadRight(definition.Length, ' ');
    }

    private static string FormatVariable(IsoFieldDefinition definition, string value)
    {
        if (value.Length > definition.Length)
        {
            throw new Iso8583InvalidFieldValueException(
                definition.Number, $"value '{value}' is longer than the maximum length {definition.Length}.");
        }

        ValidateFormat(definition, value);
        return value;
    }

    private static void ValidateFormat(IsoFieldDefinition definition, string value)
    {
        if (definition.Format == IsoFieldFormat.Numeric && !value.All(char.IsAsciiDigit))
        {
            throw new Iso8583InvalidFieldValueException(
                definition.Number, $"value '{value}' must contain digits only for a numeric field.");
        }
    }
}
