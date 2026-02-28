using FluentAssertions;

using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class JqlQueryTests
{
    [Fact(DisplayName = "Constructor throws when value is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsNullThrowsArgumentException()
    {
        // Arrange
        string value = null!;

        // Act
        Action act = () => _ = new JqlQuery(value);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor trims value")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueHasOuterWhitespaceTrimsValue()
    {
        // Arrange
        var value = "  project = APP  ";

        // Act
        var query = new JqlQuery(value);

        // Assert
        query.Value.Should().Be("project = APP");
    }

    [Fact(DisplayName = "ToString returns normalized value")]
    [Trait("Category", "Unit")]
    public void ToStringReturnsNormalizedValue()
    {
        // Arrange
        var query = new JqlQuery("status = Open");

        // Act
        var text = query.ToString();

        // Assert
        text.Should().Be("status = Open");
    }
}
