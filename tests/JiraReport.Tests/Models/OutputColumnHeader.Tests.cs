using FluentAssertions;

using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class OutputColumnHeaderTests
{
    [Fact(DisplayName = "Constructor throws when value is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenValueIsNullThrowsArgumentException()
    {
        // Arrange
        string value = null!;

        // Act
        Action act = () => _ = new OutputColumnHeader(value);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "FromFieldKey returns Field when value is blank")]
    [Trait("Category", "Unit")]
    public void FromFieldKeyWhenValueIsBlankReturnsField()
    {
        // Act
        var header = OutputColumnHeader.FromFieldKey("   ");

        // Assert
        header.Value.Should().Be("Field");
    }

    [Fact(DisplayName = "FromFieldKey normalizes underscores and spaces")]
    [Trait("Category", "Unit")]
    public void FromFieldKeyWhenValueContainsUnderscoresAndSpacesNormalizesHeader()
    {
        // Arrange
        var fieldKey = " custom_field_name ";

        // Act
        var header = OutputColumnHeader.FromFieldKey(fieldKey);

        // Assert
        header.Value.Should().Be("Custom Field Name");
    }

    [Fact(DisplayName = "ToString returns normalized value")]
    [Trait("Category", "Unit")]
    public void ToStringReturnsNormalizedValue()
    {
        // Arrange
        var header = new OutputColumnHeader("Summary");

        // Act
        var text = header.ToString();

        // Assert
        text.Should().Be("Summary");
    }
}
