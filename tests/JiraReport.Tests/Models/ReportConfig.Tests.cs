using FluentAssertions;

using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class ReportConfigTests
{
    [Fact(DisplayName = "Constructor throws when output fields are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenOutputFieldsAreNullThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyList<IssueFieldName> outputFields = null!;

        // Act
        Action act = () => _ = new ReportConfig(
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            outputFields,
            [],
            new PdfReportName("Sprint report"));

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when count fields are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCountFieldsAreNullThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyList<IssueFieldName> countFields = null!;

        // Act
        Action act = () => _ = new ReportConfig(
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            [],
            countFields,
            new PdfReportName("Sprint report"));

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor copies field collections")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValuesAreValidCopiesCollections()
    {
        // Arrange
        var outputFields = new List<IssueFieldName> { new("summary") };
        var countFields = new List<IssueFieldName> { new("status") };

        // Act
        var config = new ReportConfig(
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            outputFields,
            countFields,
            new PdfReportName("Sprint report"));

        outputFields.Add(new IssueFieldName("assignee"));
        countFields.Add(new IssueFieldName("priority"));

        // Assert
        config.Name.Value.Should().Be("Backlog");
        config.Jql.Value.Should().Be("project = APP");
        config.PdfReportName.Value.Should().Be("Sprint report");
        config.OutputFields.Select(static field => field.Value).Should().ContainSingle().Which.Should().Be("summary");
        config.CountFields.Select(static field => field.Value).Should().ContainSingle().Which.Should().Be("status");
    }
}
