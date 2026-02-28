using FluentAssertions;

using JiraReport.Models.ValueObjects;
using JiraReport.Presentation.Pdf;

using QuestPDF.Fluent;

using QLicenseType = QuestPDF.Infrastructure.LicenseType;

namespace JiraReport.Tests.Presentation.Pdf;

public sealed class PdfReportFileStoreTests
{
    [Fact(DisplayName = "Save throws when document is null")]
    [Trait("Category", "Unit")]
    public void SaveWhenDocumentIsNullThrowsArgumentNullException()
    {
        // Arrange
        var store = new PdfReportFileStore();
        QuestPDF.Infrastructure.IDocument document = null!;

        // Act
        Action act = () => store.Save(new PdfFilePath(@"C:\reports\jira.pdf"), document);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Save creates directory and writes PDF bytes")]
    [Trait("Category", "Unit")]
    public void SaveWhenDocumentIsValidCreatesDirectoryAndWritesPdfBytes()
    {
        // Arrange
        QuestPDF.Settings.License = QLicenseType.Community;
        var store = new PdfReportFileStore();
        var outputPath = new PdfFilePath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "jira-report.pdf"));
        var document = Document.Create(container =>
        {
            _ = container.Page(page => page.Content().Text("Jira report"));
        });

        try
        {
            // Act
            store.Save(outputPath, document);

            // Assert
            File.Exists(outputPath.Value).Should().BeTrue();
            new FileInfo(outputPath.Value).Length.Should().BeGreaterThan(0);
        }
        finally
        {
            var directory = Path.GetDirectoryName(outputPath.Value);
            if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
