using FluentAssertions;

using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class ReportNameTests
{
    [Fact(DisplayName = "Constructor throws when value is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsNullThrowsArgumentException()
    {
        // Arrange
        string value = null!;

        // Act
        Action act = () => _ = new ReportName(value);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor trims value")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueHasOuterWhitespaceTrimsValue()
    {
        // Arrange
        var value = "  Team backlog  ";

        // Act
        var reportName = new ReportName(value);

        // Assert
        reportName.Value.Should().Be("Team backlog");
    }

    [Fact(DisplayName = "ToString returns normalized value")]
    [Trait("Category", "Unit")]
    public void ToStringReturnsNormalizedValue()
    {
        // Arrange
        var reportName = new ReportName("Team backlog");

        // Act
        var text = reportName.ToString();

        // Assert
        text.Should().Be("Team backlog");
    }
}
