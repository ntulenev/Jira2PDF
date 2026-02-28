using System.Text.Json;

using FluentAssertions;

using JiraReport.API.Mapping;
using JiraReport.Models.ValueObjects;
using JiraReport.Transport.Models;

namespace JiraReport.Tests.API.Mapping;

public sealed class IssueMapperTests
{
    [Fact(DisplayName = "MapIssues throws when page is null")]
    [Trait("Category", "Unit")]
    public void MapIssuesWhenPageIsNullThrowsArgumentNullException()
    {
        // Arrange
        var mapper = new IssueMapper();
        JiraSearchResponse page = null!;

        // Act
        Action act = () => _ = mapper.MapIssues(page, new Dictionary<string, IReadOnlyList<string>>());

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "MapIssues throws when aliases are null")]
    [Trait("Category", "Unit")]
    public void MapIssuesWhenAliasesAreNullThrowsArgumentNullException()
    {
        // Arrange
        var mapper = new IssueMapper();
        IReadOnlyDictionary<string, IReadOnlyList<string>> aliases = null!;

        // Act
        Action act = () => _ = mapper.MapIssues(new JiraSearchResponse(), aliases);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "MapIssues returns empty list when page has no issues")]
    [Trait("Category", "Unit")]
    public void MapIssuesWhenPageHasNoIssuesReturnsEmptyList()
    {
        // Arrange
        var mapper = new IssueMapper();

        // Act
        var issues = mapper.MapIssues(new JiraSearchResponse(), new Dictionary<string, IReadOnlyList<string>>());

        // Assert
        issues.Should().BeEmpty();
    }

    [Fact(DisplayName = "MapIssues normalizes values and applies aliases")]
    [Trait("Category", "Unit")]
    public void MapIssuesWhenFieldsArePresentNormalizesValuesAndAppliesAliases()
    {
        // Arrange
        var mapper = new IssueMapper();
        var page = new JiraSearchResponse
        {
            Issues =
            [
                new JiraIssueResponse
                {
                    Key = " APP-2 ",
                    Fields = new JiraIssueFieldsResponse
                    {
                        Values = CreateFieldValues()
                    }
                },
                new JiraIssueResponse
                {
                    Key = " ",
                    Fields = new JiraIssueFieldsResponse()
                }
            ]
        };
        var aliases = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["customfield_10001"] = ["Labels"]
        };

        // Act
        var issues = mapper.MapIssues(page, aliases);

        // Assert
        issues.Should().ContainSingle();

        var issue = issues[0];
        issue.Key.Should().Be(new IssueKey("APP-2"));
        issue.GetFieldValue(new IssueKey("summary")).Value.Should().Be("Implement report");
        issue.GetFieldValue(new IssueKey("created")).Value.Should().Be("2026-02-28");
        issue.GetFieldValue(new IssueKey("issuetype")).Value.Should().Be("Bug");
        issue.GetFieldValue(new IssueKey("assignee")).Value.Should().Be("Jane Doe");
        issue.GetFieldValue(new IssueKey("customfield_10001")).Value.Should().Be("Backend, API");
        issue.GetFieldValue(new IssueKey("Labels")).Value.Should().Be("Backend, API");
        issue.GetFieldValues(new IssueKey("Labels")).Select(static value => value.Value).Should().ContainInOrder("Backend", "API");
    }

    private static Dictionary<string, JsonElement> CreateFieldValues()
    {
        using var document = JsonDocument.Parse(
            """
            {
              "summary": "  Implement report  ",
              "created": "2026-02-28T10:30:00+00:00",
              "issuetype": { "name": "Bug" },
              "assignee": { "displayName": "Jane Doe" },
              "customfield_10001": ["Backend", "backend", { "name": "API" }, "", "-"]
            }
            """);

        return document.RootElement.EnumerateObject().ToDictionary(
            static property => property.Name,
            static property => property.Value.Clone(),
            StringComparer.OrdinalIgnoreCase);
    }
}
