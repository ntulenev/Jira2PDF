using FluentAssertions;

using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class FieldValueTests
{
    [Fact(DisplayName = "Constructor throws when value is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsNullThrowsArgumentException()
    {
        // Arrange
        string value = null!;

        // Act
        Action act = () => _ = new FieldValue(value);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor trims value")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueHasOuterWhitespaceTrimsValue()
    {
        // Arrange
        var value = "  In Progress  ";

        // Act
        var fieldValue = new FieldValue(value);

        // Assert
        fieldValue.Value.Should().Be("In Progress");
    }

    [Fact(DisplayName = "Missing returns dash placeholder")]
    [Trait("Category", "Unit")]
    public void MissingReturnsDashPlaceholder()
    {
        // Act
        var fieldValue = FieldValue.Missing;

        // Assert
        fieldValue.Value.Should().Be("-");
    }

    [Fact(DisplayName = "ToString returns normalized value")]
    [Trait("Category", "Unit")]
    public void ToStringReturnsNormalizedValue()
    {
        // Arrange
        var fieldValue = new FieldValue("Summary");

        // Act
        var text = fieldValue.ToString();

        // Assert
        text.Should().Be("Summary");
    }
}
