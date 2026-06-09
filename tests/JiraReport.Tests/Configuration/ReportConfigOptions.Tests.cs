using System.ComponentModel.DataAnnotations;

using FluentAssertions;

using JiraReport.Models.Configuration;

namespace JiraReport.Tests.Configuration;

public sealed class ReportConfigOptionsTests
{
    [Fact(DisplayName = "Validator passes when required values are set")]
    [Trait("Category", "Unit")]
    public void ValidatorWhenValuesAreValidPasses()
    {
        // Arrange
        var options = new ReportConfigOptions
        {
            Name = "Backlog",
            Jql = "project = APP",
            OutputFields = ["summary"],
            OutputFieldsAliases = new Dictionary<string, string> { ["customfield_11868"] = "Sport" },
            CountFields = ["status"],
            CountFieldsAliases = new Dictionary<string, string> { ["customfield_11854"] = "Roadmap" },
            ComputedFields = new Dictionary<string, ComputedFieldOptions>
            {
                ["customfield_11728"] = new ComputedFieldOptions
                {
                    Type = "LinkedIssueProgress",
                    LinkType = "Polaris work item link",
                    Mode = "Default",
                    Metric = "IssueCount",
                    DoneStatusCategories = ["done"],
                    ChildJqlTemplate = "parent in ({keys})",
                    Format = "{PercentDone:0}% Done"
                }
            },
            FieldValueConverters = new Dictionary<string, FieldValueConverterOptions>
            {
                ["customfield_11869"] = new FieldValueConverterOptions
                {
                    Type = "JsonPath",
                    JsonPath = "end"
                }
            },
            BuildFlowTransitions = true,
            PdfReportName = "Sprint report"
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().BeEmpty();
        options.OutputFields.Should().ContainSingle().Which.Should().Be("summary");
        options.OutputFieldsAliases.Should().ContainKey("customfield_11868").WhoseValue.Should().Be("Sport");
        options.CountFields.Should().ContainSingle().Which.Should().Be("status");
        options.CountFieldsAliases.Should().ContainKey("customfield_11854").WhoseValue.Should().Be("Roadmap");
        options.ComputedFields.Should().ContainKey("customfield_11728");
        options.ComputedFields["customfield_11728"].Type.Should().Be("LinkedIssueProgress");
        options.FieldValueConverters.Should().ContainKey("customfield_11869");
        options.FieldValueConverters["customfield_11869"].JsonPath.Should().Be("end");
        options.BuildFlowTransitions.Should().BeTrue();
    }

    [Fact(DisplayName = "Validator reports missing name")]
    [Trait("Category", "Unit")]
    public void ValidatorWhenNameIsMissingReportsValidationError()
    {
        // Arrange
        var options = new ReportConfigOptions
        {
            Name = null!,
            Jql = "project = APP",
            PdfReportName = "Sprint report"
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains("Name"));
    }

    [Fact(DisplayName = "Validator reports missing JQL")]
    [Trait("Category", "Unit")]
    public void ValidatorWhenJqlIsMissingReportsValidationError()
    {
        // Arrange
        var options = new ReportConfigOptions
        {
            Name = "Backlog",
            Jql = null!,
            PdfReportName = "Sprint report"
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains("Jql"));
    }

    [Fact(DisplayName = "Validator reports missing PDF report name")]
    [Trait("Category", "Unit")]
    public void ValidatorWhenPdfReportNameIsMissingReportsValidationError()
    {
        // Arrange
        var options = new ReportConfigOptions
        {
            Name = "Backlog",
            Jql = "project = APP",
            PdfReportName = null!
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains("PdfReportName"));
    }

    private static List<ValidationResult> Validate(ReportConfigOptions options)
    {
        var results = new List<ValidationResult>();
        _ = Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
        return results;
    }
}
