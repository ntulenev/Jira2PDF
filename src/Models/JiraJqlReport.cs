namespace JiraReport.Models;

/// <summary>
/// Represents prepared report data for console and PDF output.
/// </summary>
/// <param name="Title">Report title.</param>
/// <param name="ConfigName">Optional configuration name.</param>
/// <param name="Jql">JQL query used for loading.</param>
/// <param name="GeneratedAt">Generation timestamp.</param>
/// <param name="Issues">Loaded issues.</param>
/// <param name="CountTables">Configured grouped summary tables.</param>
internal sealed record JiraJqlReport(
    string Title,
    string? ConfigName,
    string Jql,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<JiraIssue> Issues,
    IReadOnlyList<CountTable> CountTables)
{
    /// <summary>
    /// Creates report aggregate from raw issue collection.
    /// </summary>
    /// <param name="title">Report title.</param>
    /// <param name="configName">Optional config name.</param>
    /// <param name="jql">JQL query.</param>
    /// <param name="issues">Loaded issues.</param>
    /// <param name="countTables">Prepared grouped summary tables.</param>
    /// <param name="generatedAt">Generation timestamp.</param>
    /// <returns>Prepared report model.</returns>
    public static JiraJqlReport Create(
        string title,
        string? configName,
        string jql,
        IReadOnlyList<JiraIssue> issues,
        IReadOnlyList<CountTable> countTables,
        DateTimeOffset generatedAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(jql);
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentNullException.ThrowIfNull(countTables);

        return new JiraJqlReport(
            title.Trim(),
            string.IsNullOrWhiteSpace(configName) ? null : configName.Trim(),
            jql.Trim(),
            generatedAt,
            [.. issues],
            [.. countTables]);
    }
}
