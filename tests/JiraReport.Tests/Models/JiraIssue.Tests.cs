using FluentAssertions;

using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class JiraIssueTests
{
    [Fact(DisplayName = "Constructor throws when fields are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenFieldsAreNullThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyDictionary<IssueKey, FieldValue> fields = null!;

        // Act
        Action act = () => _ = new JiraIssue(new IssueKey("APP-1"), fields);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor uses empty multi value fields when none are provided")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenMultiValueFieldsAreNotProvidedUsesEmptyCollection()
    {
        // Arrange
        var fields = new Dictionary<IssueKey, FieldValue>
        {
            [new IssueKey("summary")] = new FieldValue("Implement report")
        };

        // Act
        var issue = new JiraIssue(new IssueKey("APP-1"), fields);

        // Assert
        issue.Key.Should().Be(new IssueKey("APP-1"));
        issue.Fields.Should().BeSameAs(fields);
        issue.MultiValueFields.Should().BeEmpty();
    }

    [Fact(DisplayName = "GetFieldValue returns issue key for default key")]
    [Trait("Category", "Unit")]
    public void GetFieldValueWhenFieldKeyIsDefaultReturnsIssueKey()
    {
        // Arrange
        var issue = CreateIssue();

        // Act
        var value = issue.GetFieldValue(IssueKey.DefaultKey);

        // Assert
        value.Value.Should().Be("APP-1");
    }

    [Fact(DisplayName = "GetFieldValue returns missing placeholder when field is absent")]
    [Trait("Category", "Unit")]
    public void GetFieldValueWhenFieldIsAbsentReturnsMissingPlaceholder()
    {
        // Arrange
        var issue = CreateIssue();

        // Act
        var value = issue.GetFieldValue(new IssueKey("priority"));

        // Assert
        value.Should().Be(FieldValue.Missing);
    }

    [Fact(DisplayName = "GetFieldValues returns stored multi value items")]
    [Trait("Category", "Unit")]
    public void GetFieldValuesWhenFieldIsMultiValueReturnsStoredItems()
    {
        // Arrange
        var issue = CreateIssue();

        // Act
        var values = issue.GetFieldValues(new IssueKey("labels"));

        // Assert
        values.Select(static value => value.Value).Should().ContainInOrder("Backend", "API");
    }

    [Fact(DisplayName = "GetFieldValues returns issue key for default key")]
    [Trait("Category", "Unit")]
    public void GetFieldValuesWhenFieldKeyIsDefaultReturnsIssueKey()
    {
        // Arrange
        var issue = CreateIssue();

        // Act
        var values = issue.GetFieldValues(IssueKey.DefaultKey);

        // Assert
        values.Should().ContainSingle();
        values[0].Value.Should().Be("APP-1");
    }

    [Fact(DisplayName = "GetFieldValues returns empty collection when field is absent")]
    [Trait("Category", "Unit")]
    public void GetFieldValuesWhenFieldIsAbsentReturnsEmptyCollection()
    {
        // Arrange
        var issue = CreateIssue();

        // Act
        var values = issue.GetFieldValues(new IssueKey("priority"));

        // Assert
        values.Should().BeEmpty();
    }

    private static JiraIssue CreateIssue()
    {
        return new JiraIssue(
            new IssueKey("APP-1"),
            new Dictionary<IssueKey, FieldValue>
            {
                [new IssueKey("summary")] = new FieldValue("Implement report")
            },
            new Dictionary<IssueKey, IReadOnlyList<FieldValue>>
            {
                [new IssueKey("labels")] =
                [
                    new FieldValue("Backend"),
                    new FieldValue("API")
                ]
            });
    }
}
