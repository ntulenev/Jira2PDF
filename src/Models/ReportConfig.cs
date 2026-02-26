namespace JiraReport.Models;

/// <summary>
/// Represents one named report configuration entry.
/// </summary>
/// <param name="Name">Configuration name.</param>
/// <param name="Jql">JQL query.</param>
/// <param name="OutputFields">Requested output fields.</param>
/// <param name="PdfReportName">Optional custom PDF report title.</param>
internal sealed record ReportConfig(
    string Name,
    string Jql,
    IReadOnlyList<string> OutputFields,
    string? PdfReportName);
