namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents validated JQL query value.
/// </summary>
internal readonly record struct JqlQuery
{
    /// <summary>
    /// Initializes a new <see cref="JqlQuery"/> instance.
    /// </summary>
    /// <param name="value">Raw JQL value.</param>
    public JqlQuery(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Gets normalized JQL text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Returns JQL text representation.
    /// </summary>
    /// <returns>JQL text.</returns>
    public override string ToString() => Value;
}
