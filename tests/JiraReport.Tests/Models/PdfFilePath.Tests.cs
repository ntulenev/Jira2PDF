using FluentAssertions;

using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class PdfFilePathTests
{
    [Fact(DisplayName = "Constructor throws when value is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsNullThrowsArgumentException()
    {
        // Arrange
        string value = null!;

        // Act
        Action act = () => _ = new PdfFilePath(value);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "FromReportTitle creates rooted timestamped file path")]
    [Trait("Category", "Unit")]
    public void FromReportTitleWhenTitleIsValidCreatesRootedTimestampedFilePath()
    {
        // Arrange
        var reportTitle = new PdfReportName("Sprint Report");
        var generatedAt = new DateTimeOffset(2026, 2, 28, 15, 30, 45, TimeSpan.Zero);

        // Act
        var filePath = PdfFilePath.FromReportTitle(reportTitle, generatedAt);

        // Assert
        Path.IsPathRooted(filePath.Value).Should().BeTrue();
        Path.GetFileName(filePath.Value).Should().Be("Sprint_Report_20260228_153045.pdf");
    }

    [Fact(DisplayName = "ToString returns normalized value")]
    [Trait("Category", "Unit")]
    public void ToStringReturnsNormalizedValue()
    {
        // Arrange
        var path = new PdfFilePath(@"C:\reports\jira.pdf");

        // Act
        var text = path.ToString();

        // Assert
        text.Should().Be(@"C:\reports\jira.pdf");
    }
}
