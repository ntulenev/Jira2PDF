using JiraReport.Models.ValueObjects;

using QuestPDF.Infrastructure;

namespace JiraReport.Abstractions;

/// <summary>
/// Defines persistence for generated PDF documents.
/// </summary>
internal interface IPdfReportFileStore
{
    /// <summary>
    /// Saves generated PDF document to filesystem.
    /// </summary>
    /// <param name="outputPath">Target output path.</param>
    /// <param name="document">Document instance.</param>
    void Save(PdfFilePath outputPath, IDocument document);
}
