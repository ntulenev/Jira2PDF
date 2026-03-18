using FluentAssertions;

using JiraReport.Models.Configuration;

namespace JiraReport.Tests.Configuration;

public sealed class CsvOptionsTests
{
    [Fact(DisplayName = "Constructor uses disabled defaults")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCreatedUsesDisabledDefaults()
    {
        // Arrange
        var options = new CsvOptions();

        // Assert
        options.Enabled.Should().BeFalse();
        options.DisplayHeaders.Should().BeFalse();
    }
}
