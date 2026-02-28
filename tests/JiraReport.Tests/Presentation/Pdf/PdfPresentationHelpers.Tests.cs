using FluentAssertions;

using JiraReport.Models;
using JiraReport.Models.ValueObjects;
using JiraReport.Presentation.Pdf;

using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

using QLicenseType = QuestPDF.Infrastructure.LicenseType;

namespace JiraReport.Tests.Presentation.Pdf;

public sealed class PdfPresentationHelpersTests
{
    [Fact(DisplayName = "StyleHeaderCell throws when container is null")]
    [Trait("Category", "Unit")]
    public void StyleHeaderCellWhenContainerIsNullThrowsArgumentNullException()
    {
        // Arrange
        IContainer container = null!;

        // Act
        Action act = () => _ = PdfPresentationHelpers.StyleHeaderCell(container);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "StyleBodyCell throws when container is null")]
    [Trait("Category", "Unit")]
    public void StyleBodyCellWhenContainerIsNullThrowsArgumentNullException()
    {
        // Arrange
        IContainer container = null!;

        // Act
        Action act = () => _ = PdfPresentationHelpers.StyleBodyCell(container);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Style helper methods can be used to render a document")]
    [Trait("Category", "Unit")]
    public void StyleHelperMethodsWhenUsedInDocumentRenderSuccessfully()
    {
        // Arrange
        QuestPDF.Settings.License = QLicenseType.Community;
        var document = Document.Create(container =>
        {
            _ = container.Page(page =>
            {
                page.Content().Column(column =>
                {
                    column.Item().Element(PdfPresentationHelpers.StyleHeaderCell).Text("Header");
                    column.Item().Element(PdfPresentationHelpers.StyleBodyCell).Text("Body");
                });
            });
        });

        // Act
        var bytes = document.GeneratePdf();

        // Assert
        bytes.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "BuildIssueBrowseUrl overloads build escaped browse URL")]
    [Trait("Category", "Unit")]
    public void BuildIssueBrowseUrlWhenCalledWithAllOverloadsBuildsEscapedBrowseUrl()
    {
        // Arrange
        var baseUrl = new JiraBaseUrl("https://example.test");
        var issue = new JiraIssue(new IssueKey("APP 1/2"), new Dictionary<IssueKey, FieldValue>());

        // Act
        var fromIssue = PdfPresentationHelpers.BuildIssueBrowseUrl(baseUrl, issue);
        var fromIssueKey = PdfPresentationHelpers.BuildIssueBrowseUrl(baseUrl, issue.Key);
        var fromString = PdfPresentationHelpers.BuildIssueBrowseUrl(baseUrl, " APP 1/2 ");

        // Assert
        fromIssue.Should().Be("https://example.test/browse/APP%201%2F2");
        fromIssueKey.Should().Be(fromIssue);
        fromString.Should().Be(fromIssue);
    }

    [Fact(DisplayName = "ToDateOnly formats value or returns dash")]
    [Trait("Category", "Unit")]
    public void ToDateOnlyWhenValueIsProvidedFormatsDateOtherwiseReturnsDash()
    {
        // Arrange
        var value = new DateTimeOffset(2026, 2, 28, 18, 15, 0, TimeSpan.Zero);

        // Act
        var formatted = PdfPresentationHelpers.ToDateOnly(value);
        var missing = PdfPresentationHelpers.ToDateOnly(null);

        // Assert
        formatted.Should().Be("2026-02-28");
        missing.Should().Be("-");
    }
}
