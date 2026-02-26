namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents validated issue field value.
/// </summary>
internal readonly record struct FieldValue
{
    /// <summary>
    /// Initializes a new <see cref="FieldValue"/> instance.
    /// </summary>
    /// <param name="value">Raw field value.</param>
    public FieldValue(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Gets normalized field value text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets default placeholder value for missing fields.
    /// </summary>
    public static FieldValue Missing => new("-");

    /// <summary>
    /// Returns field value text representation.
    /// </summary>
    /// <returns>Field value text.</returns>
    public override string ToString() => Value;
}
