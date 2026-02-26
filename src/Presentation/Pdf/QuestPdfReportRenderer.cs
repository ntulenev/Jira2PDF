using System.Globalization;

using JiraReport.Abstractions;
using JiraReport.Models;
using JiraReport.Models.ValueObjects;

using QuestPDF.Fluent;
using QuestPDF.Helpers;

using QLicenseType = QuestPDF.Infrastructure.LicenseType;

namespace JiraReport.Presentation.Pdf;

internal sealed class QuestPdfReportRenderer : IPdfReportRenderer
{
    public QuestPdfReportRenderer(IPdfReportFileStore pdfReportFileStore, IPdfContentComposer pdfContentComposer)
    {
        ArgumentNullException.ThrowIfNull(pdfReportFileStore);
        ArgumentNullException.ThrowIfNull(pdfContentComposer);

        _pdfReportFileStore = pdfReportFileStore;
        _pdfContentComposer = pdfContentComposer;
    }

    public void RenderReport(
        JiraJqlReport report,
        JiraBaseUrl baseUrl,
        string outputPath,
        IReadOnlyList<OutputColumn> outputColumns)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(outputColumns);

        QuestPDF.Settings.License = QLicenseType.Community;

        var document = Document.Create(container =>
        {
            _ = container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(16);
                page.DefaultTextStyle(static style => style.FontSize(9));

                page.Header().Column(column =>
                {
                    column.Spacing(2);
                    _ = column.Item().Text(report.Title).Bold().FontSize(15);
                    _ = column.Item().Text(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Generated: {0:yyyy-MM-dd HH:mm:ss zzz}",
                            report.GeneratedAt));
                    if (!string.IsNullOrWhiteSpace(report.ConfigName))
                    {
                        _ = column.Item().Text("Config: " + report.ConfigName);
                    }

                    _ = column.Item().Text("JQL: " + report.Jql);
                    _ = column.Item().Text("Total issues: " + report.Issues.Count.ToString(CultureInfo.InvariantCulture));
                    _ = column.Item().Text("Jira base URL: " + baseUrl.Value);
                });

                page.Content().PaddingTop(8).Column(column =>
                    _pdfContentComposer.ComposeContent(column, report, outputColumns, baseUrl));

                page.Footer().AlignRight().Text(text =>
                {
                    _ = text.Span("Page ");
                    _ = text.CurrentPageNumber();
                    _ = text.Span(" / ");
                    _ = text.TotalPages();
                });
            });
        });

        _pdfReportFileStore.Save(outputPath, document);
    }

    private readonly IPdfReportFileStore _pdfReportFileStore;
    private readonly IPdfContentComposer _pdfContentComposer;
}
