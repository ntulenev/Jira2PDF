using System.Text.Json;

using FluentAssertions;

using JiraReport.Transport.Models;

namespace JiraReport.Tests.Transport;

public sealed class JiraSearchResponseTests
{
    [Fact(DisplayName = "Constructor sets default values")]
    [Trait("Category", "Unit")]
    public void ConstructorSetsDefaultValues()
    {
        // Act
        var response = new JiraSearchResponse();

        // Assert
        response.Issues.Should().NotBeNull();
        response.Issues.Should().BeEmpty();
        response.IsLast.Should().BeFalse();
        response.NextPageToken.Should().BeNull();
        response.Total.Should().Be(0);
    }

    [Fact(DisplayName = "Serializer uses Jira search response property names")]
    [Trait("Category", "Unit")]
    public void SerializerWhenValuesAreSetUsesExpectedPropertyNames()
    {
        // Arrange
        var response = new JiraSearchResponse
        {
            Issues = [new JiraIssueResponse { Key = "APP-1" }],
            IsLast = true,
            NextPageToken = "page-2",
            Total = 10
        };

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        json.Should().Contain("\"issues\"");
        json.Should().Contain("\"isLast\":true");
        json.Should().Contain("\"nextPageToken\":\"page-2\"");
        json.Should().Contain("\"total\":10");
    }
}
