using JiraReport.Abstractions;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace JiraReport.Presentation.Pdf;

internal sealed class PdfReportFileStore : IPdfReportFileStore
{
    public void Save(string outputPath, IDocument document)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(document);

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }

        var pdfBytes = document.GeneratePdf();
        File.WriteAllBytes(outputPath, pdfBytes);
    }
}
