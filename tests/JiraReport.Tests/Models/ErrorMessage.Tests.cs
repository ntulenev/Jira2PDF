using FluentAssertions;

using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class ErrorMessageTests
{
    [Fact(DisplayName = "Constructor throws when value is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsNullThrowsArgumentException()
    {
        // Arrange
        string value = null!;

        // Act
        Action act = () => _ = new ErrorMessage(value);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "FromException throws when exception is null")]
    [Trait("Category", "Unit")]
    public void FromExceptionWhenExceptionIsNullThrowsArgumentNullException()
    {
        // Arrange
        Exception exception = null!;

        // Act
        Action act = () => _ = ErrorMessage.FromException(exception);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "FromException uses exception message when present")]
    [Trait("Category", "Unit")]
    public void FromExceptionWhenMessageIsPresentUsesExceptionMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("  Jira failed.  ");

        // Act
        var error = ErrorMessage.FromException(exception);

        // Assert
        error.Value.Should().Be("Jira failed.");
    }

    [Fact(DisplayName = "FromException uses fallback when message is whitespace")]
    [Trait("Category", "Unit")]
    public void FromExceptionWhenMessageIsWhitespaceUsesFallback()
    {
        // Arrange
        var exception = new InvalidOperationException(" ");

        // Act
        var error = ErrorMessage.FromException(exception);

        // Assert
        error.Value.Should().Be("Unknown error.");
    }

    [Fact(DisplayName = "ToString returns normalized value")]
    [Trait("Category", "Unit")]
    public void ToStringReturnsNormalizedValue()
    {
        // Arrange
        var error = new ErrorMessage("Request failed");

        // Act
        var text = error.ToString();

        // Assert
        text.Should().Be("Request failed");
    }
}
