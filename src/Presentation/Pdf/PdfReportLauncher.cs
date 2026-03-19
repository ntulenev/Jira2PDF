using System.Diagnostics;

using JiraReport.Abstractions;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Presentation.Pdf;

/// <summary>
/// Default implementation that opens PDF reports via the shell.
/// </summary>
internal sealed class PdfReportLauncher : IPdfReportLauncher
{
    /// <inheritdoc />
    public void Open(PdfFilePath pdfPath)
    {
        _ = Process.Start(new ProcessStartInfo
        {
            FileName = pdfPath.Value,
            UseShellExecute = true
        });
    }
}
