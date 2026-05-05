using System.Net;
using System.Text.Json;

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
        Func<Task> act = () => client.SearchIssuesAsync(new JqlQuery("project = APP"), issueFields, null, null, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Fact(DisplayName = "SearchIssuesAsync resolves fields paginates and preserves Jira result order")]
    [Trait("Category", "Unit")]
    public async Task SearchIssuesAsyncWhenJqlEndpointIsAvailablePreservesReturnedIssueOrder()
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
                    && aliases["customfield_10001"].Contains("Story Points")),
                It.IsAny<IReadOnlyDictionary<string, FieldValueConverterConfig>>()))
            .Returns(
            [
                new JiraIssue(new IssueKey("APP-2"), new Dictionary<IssueKey, FieldValue>())
            ]);

        issueMapper.Setup(m => m.MapIssues(
                secondPage,
                It.Is<IReadOnlyDictionary<string, IReadOnlyList<string>>>(aliases =>
                    aliases.ContainsKey("customfield_10001")
                    && aliases["customfield_10001"].Contains("Story Points")),
                It.IsAny<IReadOnlyDictionary<string, FieldValueConverterConfig>>()))
            .Returns(
            [
                new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue>())
            ]);

        var client = new JiraApiClient(transport.Object, Options.Create(CreateSettings(maxResultsPerPage: 25)), issueMapper.Object);

        // Act
        var issues = await client.SearchIssuesAsync(
            new JqlQuery("project = APP"),
            [new IssueFieldName("Summary"), new IssueFieldName("Story Points")],
            null,
            null,
            cts.Token);

        // Assert
        issues.Select(static issue => issue.Key.Value).Should().ContainInOrder("APP-2", "APP-1");
    }

    [Fact(DisplayName = "SearchIssuesAsync requests all custom fields with duplicate display names")]
    [Trait("Category", "Unit")]
    public async Task SearchIssuesAsyncWhenDisplayNameMatchesMultipleCustomFieldsRequestsAllCandidates()
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
                    Id = "customfield_20000",
                    Key = "customfield_20000",
                    Name = "Sport",
                    ClauseNames = ["cf[20000]", "Sport"]
                },
                new JiraFieldDefinitionResponse
                {
                    Id = "customfield_11868",
                    Key = "customfield_11868",
                    Name = "Sport",
                    ClauseNames = ["cf[11868]", "Sport"]
                }
            ]);

        var page = new JiraSearchResponse
        {
            Issues = [new JiraIssueResponse { Key = "APP-1" }],
            IsLast = true
        };

        transport.Setup(t => t.GetAsync<JiraSearchResponse>(
                It.Is<Uri>(uri =>
                    uri.OriginalString.Contains("rest/api/3/search/jql?", StringComparison.Ordinal)
                    && uri.OriginalString.Contains("fields=customfield_20000%2Ccustomfield_11868", StringComparison.Ordinal)),
                cts.Token))
            .ReturnsAsync(page);

        issueMapper.Setup(m => m.MapIssues(
                page,
                It.Is<IReadOnlyDictionary<string, IReadOnlyList<string>>>(aliases =>
                    aliases.ContainsKey("customfield_20000")
                    && aliases["customfield_20000"].Contains("Sport")
                    && aliases.ContainsKey("customfield_11868")
                    && aliases["customfield_11868"].Contains("Sport")),
                It.IsAny<IReadOnlyDictionary<string, FieldValueConverterConfig>>()))
            .Returns([new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue>())]);

        var client = new JiraApiClient(transport.Object, Options.Create(CreateSettings()), issueMapper.Object);

        // Act
        var issues = await client.SearchIssuesAsync(
            new JqlQuery("project = APP"),
            [new IssueFieldName("Sport")],
            null,
            null,
            cts.Token);

        // Assert
        issues.Should().ContainSingle();
    }

    [Fact(DisplayName = "SearchIssuesAsync computes linked issue progress from child statuses")]
    [Trait("Category", "Unit")]
    public async Task SearchIssuesAsyncWhenLinkedIssueProgressIsConfiguredComputesProgressFromChildren()
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
                    Id = "customfield_11728",
                    Key = "customfield_11728",
                    Name = "Delivery progress",
                    ClauseNames = ["cf[11728]", "Delivery progress"]
                }
            ]);

        var page = new JiraSearchResponse
        {
            Issues =
            [
                new JiraIssueResponse
                {
                    Key = "APP-1",
                    Fields = new JiraIssueFieldsResponse
                    {
                        Values = CreateFieldValues(
                            """
                            {
                              "customfield_11728": null,
                              "issuelinks": [
                                {
                                  "type": { "name": "Polaris work item link" },
                                  "inwardIssue": {
                                    "key": "APP-100",
                                    "fields": {
                                      "status": {
                                        "name": "In Progress",
                                        "statusCategory": { "key": "indeterminate" }
                                      }
                                    }
                                  }
                                }
                              ]
                            }
                            """)
                    }
                }
            ],
            IsLast = true
        };
        var children = new JiraSearchResponse
        {
            Issues =
            [
                new JiraIssueResponse
                {
                    Key = "APP-101",
                    Fields = new JiraIssueFieldsResponse
                    {
                        Values = CreateFieldValues(
                            """
                            {
                              "parent": { "key": "APP-100" },
                              "status": {
                                "name": "Done",
                                "statusCategory": { "key": "done" }
                              }
                            }
                            """)
                    }
                },
                new JiraIssueResponse
                {
                    Key = "APP-102",
                    Fields = new JiraIssueFieldsResponse
                    {
                        Values = CreateFieldValues(
                            """
                            {
                              "parent": { "key": "APP-100" },
                              "status": {
                                "name": "In Progress",
                                "statusCategory": { "key": "indeterminate" }
                              }
                            }
                            """)
                    }
                }
            ],
            IsLast = true
        };

        transport.Setup(t => t.GetAsync<JiraSearchResponse>(
                It.Is<Uri>(uri =>
                    uri.OriginalString.Contains("rest/api/3/search/jql?", StringComparison.Ordinal)
                    && uri.OriginalString.Contains("jql=project%20%3D%20APP", StringComparison.Ordinal)
                    && uri.OriginalString.Contains("fields=customfield_11728%2Cissuelinks", StringComparison.Ordinal)),
                cts.Token))
            .ReturnsAsync(page);
        transport.Setup(t => t.GetAsync<JiraSearchResponse>(
                It.Is<Uri>(uri =>
                    uri.OriginalString.Contains("rest/api/3/search/jql?", StringComparison.Ordinal)
                    && uri.OriginalString.Contains("jql=parent%20in%20%28APP-100%29", StringComparison.Ordinal)
                    && uri.OriginalString.Contains("fields=status%2Cparent", StringComparison.Ordinal)),
                cts.Token))
            .ReturnsAsync(children);

        issueMapper.Setup(m => m.MapIssues(
                It.Is<JiraSearchResponse>(searchPage =>
                    searchPage.Issues[0].Fields!.Values["customfield_11728"].GetString() == "50% Done"),
                It.Is<IReadOnlyDictionary<string, IReadOnlyList<string>>>(aliases =>
                    aliases.ContainsKey("customfield_11728")
                    && aliases["customfield_11728"].Contains("Delivery progress")),
                It.IsAny<IReadOnlyDictionary<string, FieldValueConverterConfig>>()))
            .Returns([new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue>())]);

        var computedFields = new Dictionary<string, ComputedFieldConfig>(StringComparer.OrdinalIgnoreCase)
        {
            ["customfield_11728"] = new ComputedFieldConfig(
                "LinkedIssueProgress",
                "Polaris work item link",
                "Default",
                "IssueCount",
                ["done"],
                "parent in ({keys})",
                "{PercentDone:0}% Done")
        };
        var client = new JiraApiClient(transport.Object, Options.Create(CreateSettings()), issueMapper.Object);

        // Act
        var issues = await client.SearchIssuesAsync(
            new JqlQuery("project = APP"),
            [new IssueFieldName("Delivery progress")],
            computedFields,
            null,
            cts.Token);

        // Assert
        issues.Should().ContainSingle();
    }

    [Fact(DisplayName = "SearchIssuesAsync writes zero progress when linked issue progress has no links")]
    [Trait("Category", "Unit")]
    public async Task SearchIssuesAsyncWhenLinkedIssueProgressHasNoLinksWritesZeroProgress()
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
                    Id = "customfield_11728",
                    Key = "customfield_11728",
                    Name = "Delivery progress",
                    ClauseNames = ["cf[11728]", "Delivery progress"]
                }
            ]);

        var page = new JiraSearchResponse
        {
            Issues =
            [
                new JiraIssueResponse
                {
                    Key = "APP-1",
                    Fields = new JiraIssueFieldsResponse
                    {
                        Values = CreateFieldValues(
                            """
                            {
                              "customfield_11728": null,
                              "issuelinks": []
                            }
                            """)
                    }
                }
            ],
            IsLast = true
        };

        transport.Setup(t => t.GetAsync<JiraSearchResponse>(
                It.Is<Uri>(uri =>
                    uri.OriginalString.Contains("rest/api/3/search/jql?", StringComparison.Ordinal)
                    && uri.OriginalString.Contains("jql=project%20%3D%20APP", StringComparison.Ordinal)
                    && uri.OriginalString.Contains("fields=customfield_11728%2Cissuelinks", StringComparison.Ordinal)),
                cts.Token))
            .ReturnsAsync(page);

        issueMapper.Setup(m => m.MapIssues(
                It.Is<JiraSearchResponse>(searchPage =>
                    searchPage.Issues[0].Fields!.Values["customfield_11728"].GetString() == "0%"),
                It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
                It.IsAny<IReadOnlyDictionary<string, FieldValueConverterConfig>>()))
            .Returns([new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue>())]);

        var computedFields = new Dictionary<string, ComputedFieldConfig>(StringComparer.OrdinalIgnoreCase)
        {
            ["customfield_11728"] = new ComputedFieldConfig(
                "LinkedIssueProgress",
                "Polaris work item link",
                "Default",
                "IssueCount",
                ["done"],
                "parent in ({keys})",
                "{PercentDone:0}%")
        };
        var client = new JiraApiClient(transport.Object, Options.Create(CreateSettings()), issueMapper.Object);

        // Act
        var issues = await client.SearchIssuesAsync(
            new JqlQuery("project = APP"),
            [new IssueFieldName("Delivery progress")],
            computedFields,
            null,
            cts.Token);

        // Assert
        issues.Should().ContainSingle();
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

        issueMapper.Setup(m => m.MapIssues(
                page,
                It.IsAny<IReadOnlyDictionary<string, IReadOnlyList<string>>>(),
                It.IsAny<IReadOnlyDictionary<string, FieldValueConverterConfig>>()))
            .Returns([new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue>())]);

        var client = new JiraApiClient(transport.Object, Options.Create(CreateSettings()), issueMapper.Object);

        // Act
        var issues = await client.SearchIssuesAsync(
            new JqlQuery("project = APP"),
            [new IssueFieldName("Summary")],
            null,
            null,
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
            null,
            null,
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
            [new ReportConfig(new ReportName("Backlog"), new JqlQuery("project = APP"), [], [], new PdfReportName("Sprint report"))],
            new PdfSettings(false),
            new CsvSettings(false, false));
    }

    private static Dictionary<string, JsonElement> CreateFieldValues(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject().ToDictionary(
            static property => property.Name,
            static property => property.Value.Clone(),
            StringComparer.OrdinalIgnoreCase);
    }
}
