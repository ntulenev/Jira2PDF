using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Abstractions;

/// <summary>
/// Defines domain logic for report preparation.
/// </summary>
internal interface IJiraLogicService
{
    /// <summary>
    /// Resolves output columns from config.
    /// </summary>
    /// <param name="configuredFields">Configured field names.</param>
    /// <returns>Resolved output columns.</returns>
    IReadOnlyList<OutputColumn> ResolveOutputColumns(IReadOnlyList<IssueFieldName>? configuredFields);

    /// <summary>
    /// Resolves Jira API fields to request from report field configuration.
    /// </summary>
    /// <param name="configuredOutputFields">Configured output field names.</param>
    /// <param name="configuredCountFields">Configured grouped count field names.</param>
    /// <returns>Field keys to request from Jira search API.</returns>
    IReadOnlyList<IssueFieldName> ResolveRequestedIssueFields(
        IReadOnlyList<IssueFieldName>? configuredOutputFields,
        IReadOnlyList<IssueFieldName>? configuredCountFields);

    /// <summary>
    /// Builds default output PDF path.
    /// </summary>
    /// <param name="reportTitle">Report title.</param>
    /// <param name="generatedAt">Generation timestamp.</param>
    /// <returns>Default output path.</returns>
    PdfFilePath BuildDefaultPdfPath(PdfReportName reportTitle, DateTimeOffset generatedAt);

    /// <summary>
    /// Builds report aggregate model.
    /// </summary>
    /// <param name="reportTitle">Report title.</param>
    /// <param name="configName">Configuration name.</param>
    /// <param name="jql">JQL query.</param>
    /// <param name="issues">Loaded issues.</param>
    /// <param name="configuredCountFields">Configured grouped count fields.</param>
    /// <returns>Prepared report model.</returns>
    JiraJqlReport BuildReport(
        PdfReportName reportTitle,
        ReportName configName,
        JqlQuery jql,
        IReadOnlyList<JiraIssue> issues,
        IReadOnlyList<IssueFieldName>? configuredCountFields);
}
