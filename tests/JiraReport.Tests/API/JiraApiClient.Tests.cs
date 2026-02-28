using System.Net;

using FluentAssertions;

using JiraReport.Abstractions;
using JiraReport.API;
using JiraReport.Models;
using JiraReport.Models.Configuration;
using JiraReport.Models.ValueObjects;
using JiraReport.Transport.Models;

using Microsoft.Extensions.Options;

using Moq;

namespace JiraReport.Tests.API;

public sealed class JiraApiClientTests
{
    [Fact(DisplayName = "Constructor throws when transport is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenTransportIsNullThrowsArgumentNullException()
    {
        // Arrange
        IJiraTransport transport = null!;
        var options = Options.Create(CreateSettings());
        var issueMapper = new Mock<IIssueMapper>(MockBehavior.Strict).Object;

        // Act
        Action act = () => _ = new JiraApiClient(transport, options, issueMapper);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when options are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenOptionsAreNullThrowsArgumentNullException()
    {
        // Arrange
        var transport = new Mock<IJiraTransport>(MockBehavior.Strict).Object;
        IOptions<AppSettings> options = null!;
        var issueMapper = new Mock<IIssueMapper>(MockBehavior.Strict).Object;

        // Act
        Action act = () => _ = new JiraApiClient(transport, options, issueMapper);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when issue mapper is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenIssueMapperIsNullThrowsArgumentNullException()
    {
        // Arrange
        var transport = new Mock<IJiraTransport>(MockBehavior.Strict).Object;
        var options = Options.Create(CreateSettings());
        IIssueMapper issueMapper = null!;

        // Act
        Action act = () => _ = new JiraApiClient(transport, options, issueMapper);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "SearchIssuesAsync throws when issue fields are null")]
    [Trait("Category", "Unit")]
    public async Task SearchIssuesAsyncWhenIssueFieldsAreNullThrowsArgumentNullException()
    {
        // Arrange
        var transport = new Mock<IJiraTransport>(MockBehavior.Strict).Object;
        var issueMapper = new Mock<IIssueMapper>(MockBehavior.Strict).Object;
        var client = new JiraApiClient(transport, Options.Create(CreateSettings()), issueMapper);
        IReadOnlyList<IssueFieldName> issueFields = null!;

        // Act
        Func<Task> act = () => client.SearchIssuesAsync(new JqlQuery("project = APP"), issueFields, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "SearchIssuesAsync resolves fields paginates and returns distinct sorted issues")]
    [Trait("Category", "Unit")]
    public async Task SearchIssuesAsyncWhenJqlEndpointIsAvailableReturnsDistinctSortedIssues()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var transport = new Mock<IJiraTransport>(MockBehavior.Strict);
        var issueMapper = new Mock<IIssueMapper>(MockBehavior.Strict);

        transport.Setup(t => t.GetAsync<List<JiraFieldDefinitionResponse>>(
                It.Is<Uri>(uri => uri.OriginalString == "rest/api/3/field"),
                cts.Token))
            .ReturnsAsync(
            [
                new JiraFieldDefinitionResponse
                {
                    Id = "summary",
                    Key = "summary",
                    Name = "Summary",
                    ClauseNames = ["summary"]
                },
                new JiraFieldDefinitionResponse
                {
                    Id = "customfield_10001",
                    Key = "customfield_10001",
                    Name = "Story Points",
                    ClauseNames = ["cf[10001]"]
                }
            ]);

        var firstPage = new JiraSearchResponse
        {
            Issues = [new JiraIssueResponse { Key = "APP-2" }],
            IsLast = false,
            NextPageToken = "page-2"
        };
        var secondPage = new JiraSearchResponse
        {
            Issues = [new JiraIssueResponse { Key = "APP-1" }],
            IsLast = true,
            NextPageToken = "page-3"
        };

        transport.Setup(t => t.GetAsync<JiraSearchResponse>(
                It.Is<Uri>(uri =>
                    uri.OriginalString.Contains("rest/api/3/search/jql?", StringComparison.Ordinal)
                    && uri.OriginalString.Contains("fields=summary%2Ccustomfield_10001", StringComparison.Ordinal)
                    && !uri.OriginalString.Contains("nextPageToken=", StringComparison.Ordinal)),
                cts.Token))
            .ReturnsAsync(firstPage);

        transport.Setup(t => t.GetAsync<JiraSearchResponse>(
                It.Is<Uri>(uri =>
                    uri.OriginalString.Contains("rest/api/3/search/jql?", StringComparison.Ordinal)
                    && uri.OriginalString.Contains("nextPageToken=page-2", StringComparison.Ordinal)),
                cts.Token))
            .ReturnsAsync(secondPage);

        issueMapper.Setup(m => m.MapIssues(
                firstPage,
                It.Is<IReadOnlyDictionary<string, IReadOnlyList<string>>>(aliases =>
                    aliases.ContainsKey("customfield_10001")
                    && aliases["customfield_10001"].Contains("Story Points"))))
            .Returns(
            [
                new JiraIssue(new IssueKey("APP-2"), new Dictionary<IssueKey, FieldValue>()),
                new JiraIssue(new IssueKey("app-2"), new Dictionary<IssueKey, FieldValue>())
            ]);

        issueMapper.Setup(m => m.MapIssues(
                secondPage,
                It.Is<IReadOnlyDictionary<string, IReadOnlyList<string>>>(aliases =>
                    aliases.ContainsKey("customfield_10001")
                    && aliases["customfield_10001"].Contains("Story Points"))))
            .Returns(
            [
                new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue>())
            ]);

        var client = new JiraApiClient(transport.Object, Options.Create(CreateSettings(maxResultsPerPage: 25)), issueMapper.Object);

        // Act
        var issues = await client.SearchIssuesAsync(
            new JqlQuery("project = APP"),
            [new IssueFieldName("Summary"), new IssueFieldName("Story Points")],
            cts.Token);

        // Assert
        issues.Select(static issue => issue.Key.Value).Should().ContainInOrder("APP-1", "APP-2");
    }

    [Fact(DisplayName = "SearchIssuesAsync falls back to startAt endpoint when page token endpoint is not found")]
    [Trait("Category", "Unit")]
    public async Task SearchIssuesAsyncWhenJqlEndpointIsNotFoundFallsBackToStartAtEndpoint()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var transport = new Mock<IJiraTransport>(MockBehavior.Strict);
        var issueMapper = new Mock<IIssueMapper>(MockBehavior.Strict);

        transport.Setup(t => t.GetAsync<List<JiraFieldDefinitionResponse>>(
                It.Is<Uri>(uri => uri.OriginalString == "rest/api/3/field"),
                cts.Token))
            .ReturnsAsync(
            [
                new JiraFieldDefinitionResponse
                {
                    Id = "summary",
                    Key = "summary",
                    Name = "Summary",
                    ClauseNames = ["summary"]
                }
            ]);

        transport.Setup(t => t.GetAsync<JiraSearchResponse>(
                It.Is<Uri>(uri => uri.OriginalString.Contains("rest/api/3/search/jql?", StringComparison.Ordinal)),
                cts.Token))
            .ThrowsAsync(new HttpRequestException("not found", null, HttpStatusCode.NotFound));

        var page = new JiraSearchResponse
        {
            Issues = [new JiraIssueResponse { Key = "APP-1" }],
            Total = 1
        };

        transport.Setup(t => t.GetAsync<JiraSearchResponse>(
                It.Is<Uri>(uri =>
                    uri.OriginalString.Contains("rest/api/3/search?", StringComparison.Ordinal)
                    && uri.OriginalString.Contains("startAt=0", StringComparison.Ordinal)),
                cts.Token))
            .ReturnsAsync(page);

        issueMapper.Setup(m => m.MapIssues(page, It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>()))
            .Returns([new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue>())]);

        var client = new JiraApiClient(transport.Object, Options.Create(CreateSettings()), issueMapper.Object);

        // Act
        var issues = await client.SearchIssuesAsync(
            new JqlQuery("project = APP"),
            [new IssueFieldName("Summary")],
            cts.Token);

        // Assert
        issues.Should().ContainSingle();
        issues[0].Key.Value.Should().Be("APP-1");
    }

    [Fact(DisplayName = "SearchIssuesAsync throws when configured field cannot be resolved")]
    [Trait("Category", "Unit")]
    public async Task SearchIssuesAsyncWhenConfiguredFieldCannotBeResolvedThrowsInvalidOperationException()
    {
        // Arrange
        var transport = new Mock<IJiraTransport>(MockBehavior.Strict);
        var issueMapper = new Mock<IIssueMapper>(MockBehavior.Strict);
        transport.Setup(t => t.GetAsync<List<JiraFieldDefinitionResponse>>(
                It.Is<Uri>(uri => uri.OriginalString == "rest/api/3/field"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new JiraFieldDefinitionResponse
                {
                    Id = "summary",
                    Key = "summary",
                    Name = "Summary",
                    ClauseNames = ["summary"]
                }
            ]);

        var client = new JiraApiClient(transport.Object, Options.Create(CreateSettings()), issueMapper.Object);

        // Act
        Func<Task> act = () => client.SearchIssuesAsync(
            new JqlQuery("project = APP"),
            [new IssueFieldName("Unknown Field")],
            CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>();
    }

    private static AppSettings CreateSettings(int maxResultsPerPage = 50)
    {
        return new AppSettings(
            new JiraBaseUrl("https://example.test"),
            new JiraEmail("user@example.test"),
            new JiraApiToken("token"),
            maxResultsPerPage,
            0,
            [new ReportConfig(new ReportName("Backlog"), new JqlQuery("project = APP"), [], [], new PdfReportName("Sprint report"))]);
    }
}
