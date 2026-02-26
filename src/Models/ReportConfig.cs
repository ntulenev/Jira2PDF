namespace JiraReport.Models;

internal sealed record ReportConfig(
    string Name,
    string Jql,
    IReadOnlyList<string> OutputFields,
    string? PdfReportName);
