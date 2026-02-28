using FluentAssertions;

using JiraReport.Models;
using JiraReport.Models.Configuration;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Configuration;

public sealed class AppSettingsTests
{
    [Fact(DisplayName = "Constructor sets properties")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValuesAreValidSetsProperties()
    {
        // Arrange
        var reports = new[]
        {
            new ReportConfig(
                new ReportName("Backlog"),
                new JqlQuery("project = APP"),
                [],
                [],
                new PdfReportName("Sprint report"))
        };

        // Act
        var settings = new AppSettings(
            new JiraBaseUrl("https://example.test"),
            new JiraEmail("user@example.test"),
            new JiraApiToken("token"),
            50,
            3,
            reports);

        // Assert
        settings.BaseUrl.Value.Should().Be("https://example.test");
        settings.Email.Value.Should().Be("user@example.test");
        settings.ApiToken.Value.Should().Be("token");
        settings.MaxResultsPerPage.Should().Be(50);
        settings.RetryCount.Should().Be(3);
        settings.Reports.Should().BeSameAs(reports);
    }
}
