namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents validated Jira issue field name value.
/// </summary>
internal readonly record struct IssueFieldName
{
    /// <summary>
    /// Initializes a new <see cref="IssueFieldName"/> instance.
    /// </summary>
    /// <param name="value">Raw issue field name value.</param>
    public IssueFieldName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Gets normalized issue field name text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Returns issue field name text representation.
    /// </summary>
    /// <returns>Issue field name text.</returns>
    public override string ToString() => Value;
}
