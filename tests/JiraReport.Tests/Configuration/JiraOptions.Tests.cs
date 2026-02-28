using System.ComponentModel.DataAnnotations;

using FluentAssertions;

using JiraReport.Models.Configuration;

namespace JiraReport.Tests.Configuration;

public sealed class JiraOptionsTests
{
    [Fact(DisplayName = "Validator passes when options are valid and defaults are used")]
    [Trait("Category", "Unit")]
    public void ValidatorWhenOptionsAreValidPasses()
    {
        // Arrange
        var options = new JiraOptions
        {
            BaseUrl = new Uri("https://example.test", UriKind.Absolute),
            Email = "user@example.test",
            ApiToken = "token",
            Reports =
            [
                new ReportConfigOptions
                {
                    Name = "Backlog",
                    Jql = "project = APP",
                    PdfReportName = "Sprint report"
                }
            ]
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().BeEmpty();
        options.MaxResultsPerPage.Should().Be(100);
        options.RetryCount.Should().Be(3);
    }

    [Fact(DisplayName = "Validator reports missing base URL")]
    [Trait("Category", "Unit")]
    public void ValidatorWhenBaseUrlIsMissingReportsValidationError()
    {
        // Arrange
        var options = new JiraOptions
        {
            BaseUrl = null!,
            Email = "user@example.test",
            ApiToken = "token",
            Reports =
            [
                new ReportConfigOptions
                {
                    Name = "Backlog",
                    Jql = "project = APP",
                    PdfReportName = "Sprint report"
                }
            ]
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains("BaseUrl"));
    }

    [Fact(DisplayName = "Validator reports missing email")]
    [Trait("Category", "Unit")]
    public void ValidatorWhenEmailIsMissingReportsValidationError()
    {
        // Arrange
        var options = new JiraOptions
        {
            BaseUrl = new Uri("https://example.test", UriKind.Absolute),
            Email = null!,
            ApiToken = "token",
            Reports =
            [
                new ReportConfigOptions
                {
                    Name = "Backlog",
                    Jql = "project = APP",
                    PdfReportName = "Sprint report"
                }
            ]
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains("Email"));
    }

    [Fact(DisplayName = "Validator reports missing API token")]
    [Trait("Category", "Unit")]
    public void ValidatorWhenApiTokenIsMissingReportsValidationError()
    {
        // Arrange
        var options = new JiraOptions
        {
            BaseUrl = new Uri("https://example.test", UriKind.Absolute),
            Email = "user@example.test",
            ApiToken = null!,
            Reports =
            [
                new ReportConfigOptions
                {
                    Name = "Backlog",
                    Jql = "project = APP",
                    PdfReportName = "Sprint report"
                }
            ]
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains("ApiToken"));
    }

    [Fact(DisplayName = "Validator reports invalid max results per page")]
    [Trait("Category", "Unit")]
    public void ValidatorWhenMaxResultsPerPageIsOutOfRangeReportsValidationError()
    {
        // Arrange
        var options = new JiraOptions
        {
            BaseUrl = new Uri("https://example.test", UriKind.Absolute),
            Email = "user@example.test",
            ApiToken = "token",
            MaxResultsPerPage = 101,
            Reports =
            [
                new ReportConfigOptions
                {
                    Name = "Backlog",
                    Jql = "project = APP",
                    PdfReportName = "Sprint report"
                }
            ]
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains("MaxResultsPerPage"));
    }

    [Fact(DisplayName = "Validator reports invalid retry count")]
    [Trait("Category", "Unit")]
    public void ValidatorWhenRetryCountIsOutOfRangeReportsValidationError()
    {
        // Arrange
        var options = new JiraOptions
        {
            BaseUrl = new Uri("https://example.test", UriKind.Absolute),
            Email = "user@example.test",
            ApiToken = "token",
            RetryCount = 11,
            Reports =
            [
                new ReportConfigOptions
                {
                    Name = "Backlog",
                    Jql = "project = APP",
                    PdfReportName = "Sprint report"
                }
            ]
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains("RetryCount"));
    }

    [Fact(DisplayName = "Validator reports missing reports list")]
    [Trait("Category", "Unit")]
    public void ValidatorWhenReportsAreMissingReportsValidationError()
    {
        // Arrange
        var options = new JiraOptions
        {
            BaseUrl = new Uri("https://example.test", UriKind.Absolute),
            Email = "user@example.test",
            ApiToken = "token",
            Reports = null!
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains("Reports"));
    }

    [Fact(DisplayName = "Validator reports empty reports list")]
    [Trait("Category", "Unit")]
    public void ValidatorWhenReportsAreEmptyReportsValidationError()
    {
        // Arrange
        var options = new JiraOptions
        {
            BaseUrl = new Uri("https://example.test", UriKind.Absolute),
            Email = "user@example.test",
            ApiToken = "token",
            Reports = []
        };

        // Act
        var results = Validate(options);

        // Assert
        results.Should().Contain(result => result.MemberNames.Contains("Reports"));
    }

    private static List<ValidationResult> Validate(JiraOptions options)
    {
        var results = new List<ValidationResult>();
        _ = Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
        return results;
    }
}
