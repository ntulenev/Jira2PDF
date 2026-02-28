using FluentAssertions;

using JiraReport.Models;

namespace JiraReport.Tests.Models;

public sealed class CountTableTests
{
    [Fact(DisplayName = "Constructor throws when title is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenTitleIsNullThrowsArgumentException()
    {
        // Arrange
        string title = null!;
        var rows = Array.Empty<CountRow>();

        // Act
        Action act = () => _ = new CountTable(title, rows);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "Constructor throws when rows are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenRowsAreNullThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyList<CountRow> rows = null!;

        // Act
        Action act = () => _ = new CountTable("By Status", rows);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor trims title and sets rows")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValuesAreValidSetsProperties()
    {
        // Arrange
        var rows = new[] { new CountRow("Open", 2) };

        // Act
        var table = new CountTable("  By Status  ", rows);

        // Assert
        table.Title.Should().Be("By Status");
        table.Rows.Should().BeSameAs(rows);
    }
}
