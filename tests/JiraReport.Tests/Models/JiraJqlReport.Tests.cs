using FluentAssertions;

using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class JiraJqlReportTests
{
    [Fact(DisplayName = "Constructor throws when issues are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenIssuesAreNullThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyList<JiraIssue> issues = null!;

        // Act
        Action act = () => _ = new JiraJqlReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            DateTimeOffset.UtcNow,
            issues,
            []);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when count tables are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCountTablesAreNullThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyList<CountTable> countTables = null!;

        // Act
        Action act = () => _ = new JiraJqlReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            DateTimeOffset.UtcNow,
            [],
            countTables);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor sets properties")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValuesAreValidSetsProperties()
    {
        // Arrange
        var generatedAt = new DateTimeOffset(2026, 2, 28, 16, 0, 0, TimeSpan.Zero);
        var issues = new[] { new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue>()) };
        var countTables = new[] { new CountTable("By Status", [new CountRow("Open", 1)]) };

        // Act
        var report = new JiraJqlReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            generatedAt,
            issues,
            countTables);

        // Assert
        report.Title.Value.Should().Be("Sprint report");
        report.ConfigName.Value.Should().Be("Backlog");
        report.Jql.Value.Should().Be("project = APP");
        report.GeneratedAt.Should().Be(generatedAt);
        report.Issues.Should().BeSameAs(issues);
        report.CountTables.Should().BeSameAs(countTables);
    }
}
