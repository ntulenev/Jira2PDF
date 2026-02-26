using JiraReport.Models.ValueObjects;

namespace JiraReport.Models;

/// <summary>
/// Defines a report output column.
/// </summary>
internal sealed record OutputColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OutputColumn"/> record.
    /// </summary>
    /// <param name="key">Column key.</param>
    /// <param name="header">Displayed header label.</param>
    /// <param name="selector">Value selector for an issue row.</param>
    public OutputColumn(IssueKey key, string header, Func<JiraIssue, FieldValue> selector)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(header);
        ArgumentNullException.ThrowIfNull(selector);

        Key = key;
        Header = header.Trim();
        Selector = selector;
    }

    /// <summary>
    /// Gets column key.
    /// </summary>
    public IssueKey Key { get; }

    /// <summary>
    /// Gets displayed header label.
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// Gets value selector for an issue row.
    /// </summary>
    public Func<JiraIssue, FieldValue> Selector { get; }
}
