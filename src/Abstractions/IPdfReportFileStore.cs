using QuestPDF.Infrastructure;

namespace JiraReport.Abstractions;

internal interface IPdfReportFileStore
{
    void Save(string outputPath, IDocument document);
}
