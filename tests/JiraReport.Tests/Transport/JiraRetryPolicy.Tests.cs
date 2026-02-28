using System.Net;

using FluentAssertions;

using JiraReport.Models;
using JiraReport.Models.Configuration;
using JiraReport.Models.ValueObjects;
using JiraReport.Transport;

using Microsoft.Extensions.Options;

namespace JiraReport.Tests.Transport;

public sealed class JiraRetryPolicyTests
{
    [Fact(DisplayName = "Constructor throws when options are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenOptionsAreNullThrowsArgumentNullException()
    {
        // Arrange
        IOptions<AppSettings> options = null!;

        // Act
        Action act = () => _ = new JiraRetryPolicy(options);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "TryGetDelay returns false when retry attempt is out of range")]
    [Trait("Category", "Unit")]
    public void TryGetDelayWhenRetryAttemptIsOutOfRangeReturnsFalse()
    {
        // Arrange
        var policy = new JiraRetryPolicy(Options.Create(CreateSettings(retryCount: 2)));

        // Act
        var resultZero = policy.TryGetDelay(0, HttpStatusCode.ServiceUnavailable, null, out var delayZero);
        var resultTooHigh = policy.TryGetDelay(3, HttpStatusCode.ServiceUnavailable, null, out var delayTooHigh);

        // Assert
        resultZero.Should().BeFalse();
        resultTooHigh.Should().BeFalse();
        delayZero.Should().Be(TimeSpan.Zero);
        delayTooHigh.Should().Be(TimeSpan.Zero);
    }

    [Fact(DisplayName = "TryGetDelay returns delay for HTTP request exception")]
    [Trait("Category", "Unit")]
    public void TryGetDelayWhenExceptionIsHttpRequestExceptionReturnsDelay()
    {
        // Arrange
        var policy = new JiraRetryPolicy(Options.Create(CreateSettings(retryCount: 3)));

        // Act
        var result = policy.TryGetDelay(2, null, new HttpRequestException("boom"), out var delay);

        // Assert
        result.Should().BeTrue();
        delay.Should().Be(TimeSpan.FromMilliseconds(400));
    }

    [Fact(DisplayName = "TryGetDelay returns delay for retryable status code")]
    [Trait("Category", "Unit")]
    public void TryGetDelayWhenStatusCodeIsRetryableReturnsDelay()
    {
        // Arrange
        var policy = new JiraRetryPolicy(Options.Create(CreateSettings(retryCount: 3)));

        // Act
        var result = policy.TryGetDelay(1, HttpStatusCode.ServiceUnavailable, null, out var delay);

        // Assert
        result.Should().BeTrue();
        delay.Should().Be(TimeSpan.FromMilliseconds(200));
    }

    [Fact(DisplayName = "TryGetDelay returns false for non retryable status code")]
    [Trait("Category", "Unit")]
    public void TryGetDelayWhenStatusCodeIsNotRetryableReturnsFalse()
    {
        // Arrange
        var policy = new JiraRetryPolicy(Options.Create(CreateSettings(retryCount: 3)));

        // Act
        var result = policy.TryGetDelay(1, HttpStatusCode.BadRequest, null, out var delay);

        // Assert
        result.Should().BeFalse();
        delay.Should().Be(TimeSpan.Zero);
    }

    private static AppSettings CreateSettings(int retryCount)
    {
        return new AppSettings(
            new JiraBaseUrl("https://example.test"),
            new JiraEmail("user@example.test"),
            new JiraApiToken("token"),
            50,
            retryCount,
            [new ReportConfig(new ReportName("Backlog"), new JqlQuery("project = APP"), [], [], new PdfReportName("Sprint report"))]);
    }
}
