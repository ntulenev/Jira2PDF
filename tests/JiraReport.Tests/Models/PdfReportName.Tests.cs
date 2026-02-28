using FluentAssertions;

using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class PdfReportNameTests
{
    [Fact(DisplayName = "Constructor throws when value is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsNullThrowsArgumentException()
    {
        // Arrange
        string value = null!;

        // Act
        Action act = () => _ = new PdfReportName(value);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor trims value")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueHasOuterWhitespaceTrimsValue()
    {
        // Arrange
        var value = "  Sprint report  ";

        // Act
        var reportName = new PdfReportName(value);

        // Assert
        reportName.Value.Should().Be("Sprint report");
    }

    [Fact(DisplayName = "ToString returns normalized value")]
    [Trait("Category", "Unit")]
    public void ToStringReturnsNormalizedValue()
    {
        // Arrange
        var reportName = new PdfReportName("Sprint report");

        // Act
        var text = reportName.ToString();

        // Assert
        text.Should().Be("Sprint report");
    }
}
