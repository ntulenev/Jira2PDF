using FluentAssertions;

using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class JiraBaseUrlTests
{
    [Fact(DisplayName = "Constructor throws when value is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsNullThrowsArgumentException()
    {
        // Arrange
        string value = null!;

        // Act
        Action act = () => _ = new JiraBaseUrl(value);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor throws when URL is not absolute")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenUrlIsNotAbsoluteThrowsArgumentException()
    {
        // Arrange
        var value = "/jira";

        // Act
        Action act = () => _ = new JiraBaseUrl(value);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor throws when URL scheme is unsupported")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenUrlSchemeIsUnsupportedThrowsArgumentException()
    {
        // Arrange
        var value = "ftp://example.test";

        // Act
        Action act = () => _ = new JiraBaseUrl(value);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor normalizes to authority only")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenUrlContainsPathNormalizesToAuthorityOnly()
    {
        // Arrange
        var value = "  https://example.test/jira/projects/one/  ";

        // Act
        var baseUrl = new JiraBaseUrl(value);

        // Assert
        baseUrl.Value.Should().Be("https://example.test");
    }

    [Fact(DisplayName = "ToString returns normalized URL")]
    [Trait("Category", "Unit")]
    public void ToStringReturnsNormalizedUrl()
    {
        // Arrange
        var baseUrl = new JiraBaseUrl("https://example.test");

        // Act
        var text = baseUrl.ToString();

        // Assert
        text.Should().Be("https://example.test");
    }
}
