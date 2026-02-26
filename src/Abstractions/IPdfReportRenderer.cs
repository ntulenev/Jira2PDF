using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Abstractions;

internal interface IPdfReportRenderer
{
    void RenderReport(
        JiraJqlReport report,
        JiraBaseUrl baseUrl,
        string outputPath,
        IReadOnlyList<OutputColumn> outputColumns);
}
