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
        var outputAliases = new Dictionary<string, string> { [" customfield_11868 "] = " Sport " };
        var countAliases = new Dictionary<string, string> { [" customfield_11854 "] = " Roadmap " };
        var computedFields = new Dictionary<string, ComputedFieldConfig>
        {
            [" customfield_11728 "] = new ComputedFieldConfig(
                "LinkedIssueProgress",
                "Polaris work item link",
                "Default",
                "IssueCount",
                ["done"],
                "parent in ({keys})",
                "{PercentDone:0}% Done")
        };
        var fieldValueConverters = new Dictionary<string, FieldValueConverterConfig>
        {
            [" customfield_11869 "] = new FieldValueConverterConfig("JsonPath", "end")
        };

        // Act
        var config = new ReportConfig(
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            outputFields,
            countFields,
            new PdfReportName("Sprint report"),
            outputAliases,
            countAliases,
            computedFields,
            fieldValueConverters);

        outputFields.Add(new IssueFieldName("assignee"));
        countFields.Add(new IssueFieldName("priority"));
        outputAliases["customfield_14454"] = "Domain";
        countAliases["customfield_14454"] = "Domain";
        computedFields["customfield_14454"] = new ComputedFieldConfig(
            "LinkedIssueProgress",
            "Polaris work item link",
            "Default",
            "IssueCount",
            ["done"],
            "parent in ({keys})",
            "{PercentDone:0}% Done");
        fieldValueConverters["customfield_14454"] = new FieldValueConverterConfig("JsonPath", "value");

        // Assert
        config.Name.Value.Should().Be("Backlog");
        config.Jql.Value.Should().Be("project = APP");
        config.PdfReportName.Value.Should().Be("Sprint report");
        config.OutputFields.Select(static field => field.Value).Should().ContainSingle().Which.Should().Be("summary");
        config.CountFields.Select(static field => field.Value).Should().ContainSingle().Which.Should().Be("status");
        config.OutputFieldsAliases.Should().ContainKey("customfield_11868").WhoseValue.Should().Be("Sport");
        config.OutputFieldsAliases.Should().NotContainKey("customfield_14454");
        config.CountFieldsAliases.Should().ContainKey("customfield_11854").WhoseValue.Should().Be("Roadmap");
        config.CountFieldsAliases.Should().NotContainKey("customfield_14454");
        config.ComputedFields.Should().ContainKey("customfield_11728");
        config.ComputedFields.Should().NotContainKey("customfield_14454");
        config.FieldValueConverters.Should().ContainKey("customfield_11869");
        config.FieldValueConverters["customfield_11869"].JsonPath.Should().Be("end");
        config.FieldValueConverters.Should().NotContainKey("customfield_14454");
    }
}
