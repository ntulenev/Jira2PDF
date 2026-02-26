using JiraReport.Abstractions;
using JiraReport.Models.ValueObjects;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace JiraReport.Presentation.Pdf;

/// <summary>
/// Saves generated PDF documents to filesystem.
/// </summary>
internal sealed class PdfReportFileStore : IPdfReportFileStore
{
    /// <inheritdoc />
    public void Save(PdfFilePath outputPath, IDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var path = outputPath.Value;
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }

        var pdfBytes = document.GeneratePdf();
        File.WriteAllBytes(path, pdfBytes);
    }
}
