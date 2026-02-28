using FluentAssertions;

using JiraReport.Transport;
using JiraReport.Transport.Models;

namespace JiraReport.Tests.Transport;

public sealed class SimpleJsonSerializerTests
{
    [Fact(DisplayName = "Deserialize throws when JSON is null")]
    [Trait("Category", "Unit")]
    public void DeserializeWhenJsonIsNullThrowsArgumentNullException()
    {
        // Arrange
        var serializer = new SimpleJsonSerializer();
        string json = null!;

        // Act
        Action act = () => _ = serializer.Deserialize<JiraNamedEntityResponse>(json);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Deserialize maps properties case insensitively")]
    [Trait("Category", "Unit")]
    public void DeserializeWhenJsonUsesDifferentCasingMapsPropertiesCaseInsensitively()
    {
        // Arrange
        var serializer = new SimpleJsonSerializer();
        const string json = "{\"NAME\":\"Bug\"}";

        // Act
        var result = serializer.Deserialize<JiraNamedEntityResponse>(json);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Bug");
    }

    [Fact(DisplayName = "Deserialize returns null when JSON body is null")]
    [Trait("Category", "Unit")]
    public void DeserializeWhenJsonBodyIsNullReturnsNull()
    {
        // Arrange
        var serializer = new SimpleJsonSerializer();

        // Act
        var result = serializer.Deserialize<JiraNamedEntityResponse>("null");

        // Assert
        result.Should().BeNull();
    }
}
