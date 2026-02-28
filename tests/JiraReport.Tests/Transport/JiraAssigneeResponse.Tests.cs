using System.Text.Json;

using FluentAssertions;

using JiraReport.Transport.Models;

namespace JiraReport.Tests.Transport;

public sealed class JiraAssigneeResponseTests
{
    [Fact(DisplayName = "Constructor sets property")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsAssignedSetsProperty()
    {
        // Act
        var response = new JiraAssigneeResponse
        {
            DisplayName = "Jane Doe"
        };

        // Assert
        response.DisplayName.Should().Be("Jane Doe");
    }

    [Fact(DisplayName = "Serializer uses Jira assignee property name")]
    [Trait("Category", "Unit")]
    public void SerializerWhenValueIsSetUsesExpectedPropertyName()
    {
        // Arrange
        var response = new JiraAssigneeResponse
        {
            DisplayName = "Jane Doe"
        };

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        json.Should().Contain("\"displayName\":\"Jane Doe\"");
    }
}
