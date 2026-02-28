using System.Text.Json;

using FluentAssertions;

using JiraReport.Transport.Models;

namespace JiraReport.Tests.Transport;

public sealed class JiraIssueResponseTests
{
    [Fact(DisplayName = "Constructor sets properties")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValuesAreAssignedSetsProperties()
    {
        // Arrange
        var fields = new JiraIssueFieldsResponse();

        // Act
        var response = new JiraIssueResponse
        {
            Key = "APP-1",
            Fields = fields
        };

        // Assert
        response.Key.Should().Be("APP-1");
        response.Fields.Should().BeSameAs(fields);
    }

    [Fact(DisplayName = "Serializer uses Jira issue response property names")]
    [Trait("Category", "Unit")]
    public void SerializerWhenValuesAreSetUsesExpectedPropertyNames()
    {
        // Arrange
        var response = new JiraIssueResponse
        {
            Key = "APP-1",
            Fields = new JiraIssueFieldsResponse()
        };

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        json.Should().Contain("\"key\":\"APP-1\"");
        json.Should().Contain("\"fields\"");
    }
}
