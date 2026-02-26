using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Abstractions;

/// <summary>
/// Defines PDF rendering for prepared report data.
/// </summary>
internal interface IPdfReportRenderer
{
    /// <summary>
    /// Renders report model into PDF file.
    /// </summary>
    /// <param name="report">Report model.</param>
    /// <param name="baseUrl">Jira base URL for links.</param>
    /// <param name="outputPath">Target output path.</param>
    /// <param name="outputColumns">Selected output columns.</param>
    void RenderReport(
        JiraJqlReport report,
        JiraBaseUrl baseUrl,
        PdfFilePath outputPath,
        IReadOnlyList<OutputColumn> outputColumns);
}
