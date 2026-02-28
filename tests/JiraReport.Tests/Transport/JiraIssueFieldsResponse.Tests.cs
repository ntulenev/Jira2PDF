using System.Text.Json;

using FluentAssertions;

using JiraReport.Transport.Models;

namespace JiraReport.Tests.Transport;

public sealed class JiraIssueFieldsResponseTests
{
    [Fact(DisplayName = "Constructor sets default values")]
    [Trait("Category", "Unit")]
    public void ConstructorSetsDefaultValues()
    {
        // Act
        var response = new JiraIssueFieldsResponse();

        // Assert
        response.Values.Should().NotBeNull();
        response.Values.Should().BeEmpty();
    }

    [Fact(DisplayName = "Serializer emits extension data as root properties")]
    [Trait("Category", "Unit")]
    public void SerializerWhenValuesAreSetEmitsExtensionDataAsRootProperties()
    {
        // Arrange
        using var document = JsonDocument.Parse("{\"summary\":\"Implement report\",\"points\":5}");
        var response = new JiraIssueFieldsResponse
        {
            Values = new Dictionary<string, JsonElement>
            {
                ["summary"] = document.RootElement.GetProperty("summary").Clone(),
                ["points"] = document.RootElement.GetProperty("points").Clone()
            }
        };

        // Act
        var json = JsonSerializer.Serialize(response);

        // Assert
        json.Should().Contain("\"summary\":\"Implement report\"");
        json.Should().Contain("\"points\":5");
    }
}
