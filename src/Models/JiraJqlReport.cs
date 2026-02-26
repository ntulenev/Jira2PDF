namespace JiraReport.Models;

/// <summary>
/// Represents prepared report data for console and PDF output.
/// </summary>
/// <param name="Title">Report title.</param>
/// <param name="ConfigName">Optional configuration name.</param>
/// <param name="Jql">JQL query used for loading.</param>
/// <param name="GeneratedAt">Generation timestamp.</param>
/// <param name="Issues">Loaded issues.</param>
/// <param name="ByStatus">Grouped counts by status.</param>
/// <param name="ByIssueType">Grouped counts by issue type.</param>
/// <param name="ByAssignee">Grouped counts by assignee.</param>
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
    /// <summary>
    /// Creates report aggregate from raw issue collection.
    /// </summary>
    /// <param name="title">Report title.</param>
    /// <param name="configName">Optional config name.</param>
    /// <param name="jql">JQL query.</param>
    /// <param name="issues">Loaded issues.</param>
    /// <param name="generatedAt">Generation timestamp.</param>
    /// <returns>Prepared report model.</returns>
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
