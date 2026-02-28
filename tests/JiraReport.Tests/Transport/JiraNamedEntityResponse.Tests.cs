using System.Text.Json;

using FluentAssertions;

using JiraReport.Transport.Models;

namespace JiraReport.Tests.Transport;

public sealed class JiraNamedEntityResponseTests
{
    [Fact(DisplayName = "Constructor sets property")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsAssignedSetsProperty()
    {
        // Act
        var response = new JiraNamedEntityResponse
        {
            Name = "Bug"
        };

        // Assert
        response.Name.Should().Be("Bug");
    }

    [Fact(DisplayName = "Serializer uses Jira named entity property name")]
    [Trait("Category", "Unit")]
    public void SerializerWhenValueIsSetUsesExpectedPropertyName()
    {
        // Arrange
        var response = new JiraNamedEntityResponse
        {
            Name = "Bug"
        };

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        json.Should().Contain("\"name\":\"Bug\"");
    }
}
