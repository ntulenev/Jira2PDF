namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents Jira email value.
/// </summary>
internal readonly record struct JiraEmail
{
    /// <summary>
    /// Initializes a new <see cref="JiraEmail"/> instance.
    /// </summary>
    /// <param name="value">Raw email value.</param>
    public JiraEmail(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Gets normalized email text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Returns email text representation.
    /// </summary>
    /// <returns>Email text.</returns>
    public override string ToString() => Value;
}
