namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents validated Jira issue key value.
/// </summary>
internal readonly record struct IssueKey
{
    /// <summary>
    /// Initializes a new <see cref="IssueKey"/> instance.
    /// </summary>
    /// <param name="value">Raw issue key value.</param>
    public IssueKey(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Gets normalized issue key text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the reserved field key that represents issue key.
    /// </summary>
    public static IssueKey DefaultKey => new("key");

    /// <inheritdoc />
    public bool Equals(IssueKey other) =>
        string.Equals(Value, other.Value, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

    /// <summary>
    /// Returns issue key text representation.
    /// </summary>
    /// <returns>Issue key text.</returns>
    public override string ToString() => Value;
}
