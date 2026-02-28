using FluentAssertions;

using JiraReport.Models;
using JiraReport.Models.ValueObjects;
using JiraReport.Presentation.Pdf;

using QuestPDF.Fluent;

using QLicenseType = QuestPDF.Infrastructure.LicenseType;

namespace JiraReport.Tests.Presentation.Pdf;

public sealed class PdfContentComposerTests
{
    [Fact(DisplayName = "ComposeContent throws when column is null")]
    [Trait("Category", "Unit")]
    public void ComposeContentWhenColumnIsNullThrowsArgumentNullException()
    {
        // Arrange
        var composer = new PdfContentComposer();
        QuestPDF.Fluent.ColumnDescriptor column = null!;
        var report = CreateReport();
        var outputColumns = CreateOutputColumns();

        // Act
        Action act = () => composer.ComposeContent(column, report, outputColumns, new JiraBaseUrl("https://example.test"));

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ComposeContent throws when report is null")]
    [Trait("Category", "Unit")]
    public void ComposeContentWhenReportIsNullThrowsArgumentNullException()
    {
        // Arrange
        var composer = new PdfContentComposer();
        JiraJqlReport report = null!;
        var outputColumns = CreateOutputColumns();

        // Act
        Action act = () => RenderDocument(column => composer.ComposeContent(column, report, outputColumns, new JiraBaseUrl("https://example.test")));

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ComposeContent throws when output columns are null")]
    [Trait("Category", "Unit")]
    public void ComposeContentWhenOutputColumnsAreNullThrowsArgumentNullException()
    {
        // Arrange
        var composer = new PdfContentComposer();
        var report = CreateReport();
        IReadOnlyList<OutputColumn> outputColumns = null!;

        // Act
        Action act = () => RenderDocument(column => composer.ComposeContent(column, report, outputColumns, new JiraBaseUrl("https://example.test")));

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "ComposeContent renders summary and issues tables")]
    [Trait("Category", "Unit")]
    public void ComposeContentWhenValuesAreValidRendersSummaryAndIssuesTables()
    {
        // Arrange
        var composer = new PdfContentComposer();
        var report = CreateReport();
        var outputColumns = CreateOutputColumns();

        // Act
        var bytes = RenderDocument(column => composer.ComposeContent(column, report, outputColumns, new JiraBaseUrl("https://example.test")));

        // Assert
        bytes.Should().NotBeEmpty();
    }

    private static byte[] RenderDocument(Action<QuestPDF.Fluent.ColumnDescriptor> compose)
    {
        QuestPDF.Settings.License = QLicenseType.Community;
        var document = Document.Create(container =>
        {
            _ = container.Page(page =>
            {
                page.Content().Column(column => compose(column));
            });
        });

        return document.GeneratePdf();
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
