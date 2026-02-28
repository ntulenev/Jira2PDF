using FluentAssertions;

using JiraReport.Abstractions;
using JiraReport.Models;
using JiraReport.Models.ValueObjects;
using JiraReport.Presentation.Pdf;

using Moq;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace JiraReport.Tests.Presentation.Pdf;

public sealed class QuestPdfReportRendererTests
{
    [Fact(DisplayName = "Constructor throws when file store is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenFileStoreIsNullThrowsArgumentNullException()
    {
        // Arrange
        IPdfReportFileStore fileStore = null!;
        var composer = new Mock<IPdfContentComposer>(MockBehavior.Strict).Object;

        // Act
        Action act = () => _ = new QuestPdfReportRenderer(fileStore, composer);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when content composer is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenContentComposerIsNullThrowsArgumentNullException()
    {
        // Arrange
        var fileStore = new Mock<IPdfReportFileStore>(MockBehavior.Strict).Object;
        IPdfContentComposer composer = null!;

        // Act
        Action act = () => _ = new QuestPdfReportRenderer(fileStore, composer);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "RenderReport throws when report is null")]
    [Trait("Category", "Unit")]
    public void RenderReportWhenReportIsNullThrowsArgumentNullException()
    {
        // Arrange
        var fileStore = new Mock<IPdfReportFileStore>(MockBehavior.Strict).Object;
        var composer = new Mock<IPdfContentComposer>(MockBehavior.Strict).Object;
        var renderer = new QuestPdfReportRenderer(fileStore, composer);
        JiraJqlReport report = null!;

        // Act
        Action act = () => renderer.RenderReport(
            report,
            new JiraBaseUrl("https://example.test"),
            new PdfFilePath(@"C:\reports\jira.pdf"),
            []);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "RenderReport throws when output columns are null")]
    [Trait("Category", "Unit")]
    public void RenderReportWhenOutputColumnsAreNullThrowsArgumentNullException()
    {
        // Arrange
        var fileStore = new Mock<IPdfReportFileStore>(MockBehavior.Strict).Object;
        var composer = new Mock<IPdfContentComposer>(MockBehavior.Strict).Object;
        var renderer = new QuestPdfReportRenderer(fileStore, composer);
        IReadOnlyList<OutputColumn> outputColumns = null!;

        // Act
        Action act = () => renderer.RenderReport(
            CreateReport(),
            new JiraBaseUrl("https://example.test"),
            new PdfFilePath(@"C:\reports\jira.pdf"),
            outputColumns);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "RenderReport composes document and saves it")]
    [Trait("Category", "Unit")]
    public void RenderReportWhenValuesAreValidComposesDocumentAndSavesIt()
    {
        // Arrange
        var report = CreateReport();
        var baseUrl = new JiraBaseUrl("https://example.test");
        var outputPath = new PdfFilePath(@"C:\reports\jira.pdf");
        var outputColumns = CreateOutputColumns();
        var composeCalls = 0;
        var saveCalls = 0;

        var composer = new Mock<IPdfContentComposer>(MockBehavior.Strict);
        composer.Setup(service => service.ComposeContent(It.IsAny<QuestPDF.Fluent.ColumnDescriptor>(), report, outputColumns, baseUrl))
            .Callback(() => composeCalls++);

        var fileStore = new Mock<IPdfReportFileStore>(MockBehavior.Strict);
        fileStore.Setup(store => store.Save(outputPath, It.IsAny<IDocument>()))
            .Callback<PdfFilePath, IDocument>((_, document) =>
            {
                saveCalls++;
                var bytes = document.GeneratePdf();
                bytes.Should().NotBeEmpty();
            });

        var renderer = new QuestPdfReportRenderer(fileStore.Object, composer.Object);

        // Act
        renderer.RenderReport(report, baseUrl, outputPath, outputColumns);

        // Assert
        composeCalls.Should().Be(1);
        saveCalls.Should().Be(1);
    }

    private static JiraJqlReport CreateReport()
    {
        return new JiraJqlReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            new DateTimeOffset(2026, 2, 28, 18, 0, 0, TimeSpan.Zero),
            [new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue> { [new IssueKey("summary")] = new FieldValue("Implement report") })],
            [new CountTable("By Status", [new CountRow("Open", 1)])]);
    }

    private static IReadOnlyList<OutputColumn> CreateOutputColumns()
    {
        return
        [
            new OutputColumn(IssueKey.DefaultKey, new OutputColumnHeader("Key"), static issue => issue.GetFieldValue(IssueKey.DefaultKey)),
            new OutputColumn(new IssueKey("summary"), new OutputColumnHeader("Summary"), static issue => issue.GetFieldValue(new IssueKey("summary")))
        ];
    }
}
