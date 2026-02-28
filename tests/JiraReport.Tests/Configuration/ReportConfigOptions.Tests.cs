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
            CountFields = ["status"],
            PdfReportName = "Sprint report"
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().BeEmpty();
        options.OutputFields.Should().ContainSingle().Which.Should().Be("summary");
        options.CountFields.Should().ContainSingle().Which.Should().Be("status");
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
