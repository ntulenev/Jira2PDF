namespace JiraReport.Models;

/// <summary>
/// Represents issue data used in report output.
/// </summary>
/// <param name="Key">Issue key.</param>
/// <param name="Summary">Issue summary text.</param>
/// <param name="Status">Issue status name.</param>
/// <param name="IssueType">Issue type name.</param>
/// <param name="Assignee">Assignee display name.</param>
/// <param name="Created">Issue creation timestamp.</param>
/// <param name="Updated">Issue update timestamp.</param>
internal sealed record JiraIssue(
    string Key,
    string Summary,
    string Status,
    string IssueType,
    string Assignee,
    DateTimeOffset? Created,
    DateTimeOffset? Updated);
