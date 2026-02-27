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
    /// <param name="multiValueFields">Normalized multi-value field items by key.</param>
    public JiraIssue(
        IssueKey key,
        IReadOnlyDictionary<IssueKey, FieldValue> fields,
        IReadOnlyDictionary<IssueKey, IReadOnlyList<FieldValue>>? multiValueFields = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key.Value);
        ArgumentNullException.ThrowIfNull(fields);

        Key = key;
        Fields = fields;
        MultiValueFields = multiValueFields ?? _emptyMultiValueFields;
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
    /// Gets normalized multi-value field items by key.
    /// </summary>
    public IReadOnlyDictionary<IssueKey, IReadOnlyList<FieldValue>> MultiValueFields { get; }

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

    /// <summary>
    /// Gets normalized multi-value field items by key.
    /// </summary>
    /// <param name="fieldKey">Field key.</param>
    /// <returns>Field item values or empty list if field is not multi-value or missing.</returns>
    public IReadOnlyList<FieldValue> GetFieldValues(IssueKey fieldKey)
    {
        if (fieldKey == IssueKey.DefaultKey)
        {
            return [new FieldValue(Key.Value)];
        }

        return MultiValueFields.TryGetValue(fieldKey, out var values) ? values : _emptyFieldValues;
    }

    private static readonly IReadOnlyDictionary<IssueKey, IReadOnlyList<FieldValue>> _emptyMultiValueFields =
        new Dictionary<IssueKey, IReadOnlyList<FieldValue>>();
    private static readonly IReadOnlyList<FieldValue> _emptyFieldValues = [];
}
