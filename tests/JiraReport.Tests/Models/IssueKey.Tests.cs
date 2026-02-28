using FluentAssertions;

using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class IssueKeyTests
{
    [Fact(DisplayName = "Constructor throws when value is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsNullThrowsArgumentException()
    {
        // Arrange
        string value = null!;

        // Act
        Action act = () => _ = new IssueKey(value);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor trims value")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueHasOuterWhitespaceTrimsValue()
    {
        // Arrange
        var value = "  customfield_12345  ";

        // Act
        var issueKey = new IssueKey(value);

        // Assert
        issueKey.Value.Should().Be("customfield_12345");
    }

    [Fact(DisplayName = "DefaultKey returns key field name")]
    [Trait("Category", "Unit")]
    public void DefaultKeyReturnsKeyFieldName()
    {
        // Act
        var issueKey = IssueKey.DefaultKey;

        // Assert
        issueKey.Value.Should().Be("key");
    }

    [Fact(DisplayName = "Equality ignores casing")]
    [Trait("Category", "Unit")]
    public void EqualsWhenValuesDifferByCasingReturnsTrue()
    {
        // Arrange
        var first = new IssueKey("Summary");
        var second = new IssueKey("summary");

        // Act
        var areEqual = first == second;

        // Assert
        areEqual.Should().BeTrue();
        first.GetHashCode().Should().Be(second.GetHashCode());
    }

    [Fact(DisplayName = "ToString returns normalized value")]
    [Trait("Category", "Unit")]
    public void ToStringReturnsNormalizedValue()
    {
        // Arrange
        var issueKey = new IssueKey("ABC-1");

        // Act
        var text = issueKey.ToString();

        // Assert
        text.Should().Be("ABC-1");
    }
}
