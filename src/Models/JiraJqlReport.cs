using JiraReport.Models.ValueObjects;

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
    /// <param name="flowPathGroups"></param>
    public JiraJqlReport(
        PdfReportName title,
        ReportName configName,
        JqlQuery jql,
        DateTimeOffset generatedAt,
        IReadOnlyList<JiraIssue> issues,
        IReadOnlyList<CountTable> countTables,
        IReadOnlyList<FlowPathGroup>? flowPathGroups = null)
    {
        ArgumentNullException.ThrowIfNull(issues);
        ArgumentNullException.ThrowIfNull(countTables);

        Title = title;
        ConfigName = configName;
        Jql = jql;
        GeneratedAt = generatedAt;
        Issues = issues;
        CountTables = countTables;
        FlowPathGroups = flowPathGroups ?? [];
    }

    /// <summary>
    /// Gets report title.
    /// </summary>
    public PdfReportName Title { get; }

    /// <summary>
    /// Gets configuration name.
    /// </summary>
    public ReportName ConfigName { get; }

    /// <summary>
    /// Gets JQL query used for loading.
    /// </summary>
    public JqlQuery Jql { get; }

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

    /// <summary>Gets PDF-only workflow path analytics.</summary>
    public IReadOnlyList<FlowPathGroup> FlowPathGroups { get; }
}
