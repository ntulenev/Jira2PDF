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
        OpenFile(pdfPath.Value);
    }

    /// <inheritdoc />
    public void Open(CsvFilePath csvPath)
    {
        OpenFile(csvPath.Value);
    }

    private static void OpenFile(string filePath)
    {
        _ = Process.Start(new ProcessStartInfo
        {
            FileName = filePath,
            UseShellExecute = true
        });
    }
}
