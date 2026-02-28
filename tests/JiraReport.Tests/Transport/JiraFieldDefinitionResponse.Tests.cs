using System.Text.Json;

using FluentAssertions;

using JiraReport.Transport.Models;

namespace JiraReport.Tests.Transport;

public sealed class JiraFieldDefinitionResponseTests
{
    [Fact(DisplayName = "Constructor sets default values")]
    [Trait("Category", "Unit")]
    public void ConstructorSetsDefaultValues()
    {
        // Act
        var response = new JiraFieldDefinitionResponse();

        // Assert
        response.Id.Should().BeNull();
        response.Key.Should().BeNull();
        response.Name.Should().BeNull();
        response.ClauseNames.Should().NotBeNull();
        response.ClauseNames.Should().BeEmpty();
    }

    [Fact(DisplayName = "Serializer uses Jira field definition property names")]
    [Trait("Category", "Unit")]
    public void SerializerWhenValuesAreSetUsesExpectedPropertyNames()
    {
        // Arrange
        var response = new JiraFieldDefinitionResponse
        {
            Id = "customfield_10001",
            Key = "customfield_10001",
            Name = "Story Points",
            ClauseNames = ["cf[10001]"]
        };

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        json.Should().Contain("\"id\":\"customfield_10001\"");
        json.Should().Contain("\"key\":\"customfield_10001\"");
        json.Should().Contain("\"name\":\"Story Points\"");
        json.Should().Contain("\"clauseNames\"");
    }
}
