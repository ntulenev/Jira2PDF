using FluentAssertions;

using JiraReport.Models;

namespace JiraReport.Tests.Models;

public sealed class CountRowTests
{
    [Fact(DisplayName = "Constructor throws when name is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenNameIsNullThrowsArgumentException()
    {
        // Arrange
        string name = null!;

        // Act
        Action act = () => _ = new CountRow(name, 1);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor throws when count is negative")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCountIsNegativeThrowsArgumentOutOfRangeException()
    {
        // Arrange
        const int count = -1;

        // Act
        Action act = () => _ = new CountRow("Open", count);

        // Assert
        act.Should()
            .Throw<ArgumentOutOfRangeException>();
    }

    [Fact(DisplayName = "Constructor trims name and sets count")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValuesAreValidSetsProperties()
    {
        // Arrange
        const string name = "  Open  ";
        const int count = 3;

        // Act
        var row = new CountRow(name, count);

        // Assert
        row.Name.Should().Be("Open");
        row.Count.Should().Be(count);
    }
}
