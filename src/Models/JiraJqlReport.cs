namespace JiraReport.Models;

internal sealed record JiraJqlReport(
    string Title,
    string? ConfigName,
    string Jql,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<JiraIssue> Issues,
    IReadOnlyList<CountRow> ByStatus,
    IReadOnlyList<CountRow> ByIssueType,
    IReadOnlyList<CountRow> ByAssignee)
{
    public static JiraJqlReport Create(
        string title,
        string? configName,
        string jql,
        IReadOnlyList<JiraIssue> issues,
        DateTimeOffset generatedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(jql);
        ArgumentNullException.ThrowIfNull(issues);

        return new JiraJqlReport(
            title.Trim(),
            string.IsNullOrWhiteSpace(configName) ? null : configName.Trim(),
            jql.Trim(),
            generatedAt,
            [.. issues],
            GroupByCount(issues, static issue => issue.Status),
            GroupByCount(issues, static issue => issue.IssueType),
            GroupByCount(issues, static issue => issue.Assignee));
    }

    private static IReadOnlyList<CountRow> GroupByCount(
        IReadOnlyList<JiraIssue> issues,
        Func<JiraIssue, string> selector)
    {
        return [.. issues
            .GroupBy(
                issue =>
                {
                    var value = selector(issue);
                    return string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();
                },
                StringComparer.OrdinalIgnoreCase)
            .Select(static group => new CountRow(group.Key, group.Count()))
            .OrderByDescending(static group => group.Count)
            .ThenBy(static group => group.Name, StringComparer.OrdinalIgnoreCase)];
    }
}
