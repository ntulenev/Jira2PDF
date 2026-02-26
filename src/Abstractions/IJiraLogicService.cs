using JiraReport.Models;

namespace JiraReport.Abstractions;

/// <summary>
/// Defines domain logic for report preparation.
/// </summary>
internal interface IJiraLogicService
{
    /// <summary>
    /// Resolves visible report title.
    /// </summary>
    /// <param name="selectedReportConfig">Selected report config or null.</param>
    /// <returns>Resolved title text.</returns>
    string ResolveReportTitle(ReportConfig? selectedReportConfig);

    /// <summary>
    /// Resolves output columns from config.
    /// </summary>
    /// <param name="configuredFields">Configured field names.</param>
    /// <returns>Resolved output columns.</returns>
    IReadOnlyList<OutputColumn> ResolveOutputColumns(IReadOnlyList<string>? configuredFields);

    /// <summary>
    /// Resolves Jira API fields to request from report field configuration.
    /// </summary>
    /// <param name="configuredOutputFields">Configured output field names.</param>
    /// <param name="configuredCountFields">Configured grouped count field names.</param>
    /// <returns>Field keys to request from Jira search API.</returns>
    IReadOnlyList<string> ResolveRequestedIssueFields(
        IReadOnlyList<string>? configuredOutputFields,
        IReadOnlyList<string>? configuredCountFields);

    /// <summary>
    /// Builds default output PDF path.
    /// </summary>
    /// <param name="configuredPath">Configured base path.</param>
    /// <param name="reportTitle">Report title.</param>
    /// <param name="generatedAt">Generation timestamp.</param>
    /// <returns>Default output path.</returns>
    string BuildDefaultPdfPath(string configuredPath, string reportTitle, DateTimeOffset generatedAt);

    /// <summary>
    /// Builds report aggregate model.
    /// </summary>
    /// <param name="reportTitle">Report title.</param>
    /// <param name="configName">Optional config name.</param>
    /// <param name="jql">JQL query.</param>
    /// <param name="issues">Loaded issues.</param>
    /// <param name="configuredCountFields">Configured grouped count fields.</param>
    /// <returns>Prepared report model.</returns>
    JiraJqlReport BuildReport(
        string reportTitle,
        string? configName,
        string jql,
        IReadOnlyList<JiraIssue> issues,
        IReadOnlyList<string>? configuredCountFields);
}
