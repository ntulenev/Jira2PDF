using JiraReport.Models.ValueObjects;

namespace JiraReport.Abstractions;

/// <summary>
/// Opens generated report files in the system default application.
/// </summary>
internal interface IPdfReportLauncher
{
    /// <summary>
    /// Opens the generated PDF report.
    /// </summary>
    /// <param name="pdfPath">Absolute path to generated PDF report.</param>
    void Open(PdfFilePath pdfPath);

    /// <summary>
    /// Opens the generated CSV report.
    /// </summary>
    /// <param name="csvPath">Absolute path to generated CSV report.</param>
    void Open(CsvFilePath csvPath);
}
