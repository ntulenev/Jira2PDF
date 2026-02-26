using JiraReport.Models.ValueObjects;

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
    public JiraIssue(IssueKey key, IReadOnlyDictionary<IssueKey, FieldValue> fields)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key.Value);
        ArgumentNullException.ThrowIfNull(fields);

        Key = key;
        Fields = fields;
    }

    /// <summary>
    /// Gets issue key.
    /// </summary>
    public IssueKey Key { get; }

    /// <summary>
    /// Gets normalized field values by key.
    /// </summary>
    public IReadOnlyDictionary<IssueKey, FieldValue> Fields { get; }

    /// <summary>
    /// Gets normalized field value by key.
    /// </summary>
    /// <param name="fieldKey">Field key.</param>
    /// <returns>Field value or dash if missing.</returns>
    public FieldValue GetFieldValue(IssueKey fieldKey)
    {
        if (fieldKey == IssueKey.DefaultKey)
        {
            return new FieldValue(Key.Value);
        }

        return Fields.TryGetValue(fieldKey, out var value) ? value : FieldValue.Missing;
    }
}
