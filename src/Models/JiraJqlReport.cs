namespace JiraReport.Models;

/// <summary>
/// Represents prepared report data for console and PDF output.
/// </summary>
internal sealed record JiraJqlReport
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JiraJqlReport"/> record.
    /// </summary>
    /// <param name="title">Report title.</param>
    /// <param name="configName">Configuration name.</param>
    /// <param name="jql">JQL query.</param>
    /// <param name="generatedAt">Generation timestamp.</param>
    /// <param name="issues">Loaded issues.</param>
    /// <param name="countTables">Prepared grouped summary tables.</param>
    public JiraJqlReport(
        string title,
        string configName,
        string jql,
        DateTimeOffset generatedAt,
        IReadOnlyList<JiraIssue> issues,
        IReadOnlyList<CountTable> countTables)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(configName);
        ArgumentException.ThrowIfNullOrWhiteSpace(jql);
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentNullException.ThrowIfNull(countTables);

        Title = title.Trim();
        ConfigName = configName.Trim();
        Jql = jql.Trim();
        GeneratedAt = generatedAt;
        Issues = [.. issues];
        CountTables = [.. countTables];
    }

    /// <summary>
    /// Gets report title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets configuration name.
    /// </summary>
    public string ConfigName { get; }

    /// <summary>
    /// Gets JQL query used for loading.
    /// </summary>
    public string Jql { get; }

    /// <summary>
    /// Gets generation timestamp.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; }

    /// <summary>
    /// Gets loaded issues.
    /// </summary>
    public IReadOnlyList<JiraIssue> Issues { get; }

    /// <summary>
    /// Gets configured grouped summary tables.
    /// </summary>
    public IReadOnlyList<CountTable> CountTables { get; }
}
