namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents Jira API token value.
/// </summary>
internal readonly record struct JiraApiToken
{
    /// <summary>
    /// Initializes a new <see cref="JiraApiToken"/> instance.
    /// </summary>
    /// <param name="value">Raw token value.</param>
    public JiraApiToken(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Gets normalized token text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Returns token text representation.
    /// </summary>
    /// <returns>Token text.</returns>
    public override string ToString() => Value;
}
