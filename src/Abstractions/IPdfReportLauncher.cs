using JiraReport.Models.ValueObjects;

namespace JiraReport.Abstractions;

/// <summary>
/// Opens generated PDF reports in the system default application.
/// </summary>
internal interface IPdfReportLauncher
{
    /// <summary>
    /// Opens the generated PDF report.
    /// </summary>
    /// <param name="pdfPath">Absolute path to generated PDF report.</param>
    void Open(PdfFilePath pdfPath);
}
