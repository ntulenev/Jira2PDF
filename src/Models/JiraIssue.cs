namespace JiraReport.Models;

internal sealed record JiraIssue(
    string Key,
    string Summary,
    string Status,
    string IssueType,
    string Assignee,
    DateTimeOffset? Created,
    DateTimeOffset? Updated);
