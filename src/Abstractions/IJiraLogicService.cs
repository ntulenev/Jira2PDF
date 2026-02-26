using JiraReport.Models;

namespace JiraReport.Abstractions;

internal interface IJiraLogicService
{
    string ResolveReportTitle(ReportConfig? selectedReportConfig);

    IReadOnlyList<OutputColumn> ResolveOutputColumns(IReadOnlyList<string>? configuredFields);

    string BuildDefaultPdfPath(string configuredPath, string reportTitle, DateTimeOffset generatedAt);

    JiraJqlReport BuildReport(
        string reportTitle,
        string? configName,
        string jql,
        IReadOnlyList<JiraIssue> issues);
}
