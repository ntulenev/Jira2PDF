using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Abstractions;

internal interface IJiraPresentationService
{
    ReportConfig? SelectReportConfig(IReadOnlyList<ReportConfig> sourceReports);

    string ResolveJql(IReadOnlyList<string> args);

    string ResolvePdfPath(string defaultPdfPath);

    void ShowReport(JiraJqlReport report, IReadOnlyList<OutputColumn> outputColumns);

    void ShowPdfSaved(string pdfPath);

    void ShowError(ErrorMessage errorMessage);

    Task<T> RunLoadingAsync<T>(string title, Func<Action<string>, Task<T>> action);

    Task RunLoadingAsync(string title, Func<Action<string>, Task> action);
}
