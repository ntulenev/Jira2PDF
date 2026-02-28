using FluentAssertions;

using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class OutputColumnTests
{
    [Fact(DisplayName = "Constructor throws when selector is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenSelectorIsNullThrowsArgumentNullException()
    {
        // Arrange
        Func<JiraIssue, FieldValue> selector = null!;

        // Act
        Action act = () => _ = new OutputColumn(new IssueKey("summary"), new OutputColumnHeader("Summary"), selector);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor sets properties")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValuesAreValidSetsProperties()
    {
        // Arrange
        var key = new IssueKey("summary");
        var header = new OutputColumnHeader("Summary");
        Func<JiraIssue, FieldValue> selector = static issue => issue.GetFieldValue(new IssueKey("summary"));

        // Act
        var column = new OutputColumn(key, header, selector);

        // Assert
        column.Key.Should().Be(key);
        column.Header.Should().Be(header);
        column.Selector.Should().BeSameAs(selector);
    }
}
