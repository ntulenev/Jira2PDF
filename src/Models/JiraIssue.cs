namespace JiraReport.Models;

/// <summary>
/// Represents issue data used in report output.
/// </summary>
internal sealed record JiraIssue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JiraIssue"/> record.
    /// </summary>
    /// <param name="key">Issue key.</param>
    /// <param name="fields">Normalized field values by key.</param>
    public JiraIssue(string key, IReadOnlyDictionary<string, string> fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(fields);

        Key = key.Trim();
        Fields = fields;
    }

    /// <summary>
    /// Gets issue key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets normalized field values by key.
    /// </summary>
    public IReadOnlyDictionary<string, string> Fields { get; }

    /// <summary>
    /// Gets normalized field value by key.
    /// </summary>
    /// <param name="fieldKey">Field key.</param>
    /// <returns>Field value or dash if missing.</returns>
    public string GetFieldValue(string fieldKey)
    {
        if (string.IsNullOrWhiteSpace(fieldKey))
        {
            return "-";
        }

        if (string.Equals(fieldKey, "key", StringComparison.OrdinalIgnoreCase))
        {
            return Key;
        }

        return Fields.TryGetValue(fieldKey, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : "-";
    }
}
