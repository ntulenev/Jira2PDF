using FluentAssertions;

using JiraReport.Models.Configuration;

namespace JiraReport.Tests.Configuration;

public sealed class PdfOptionsTests
{
    [Fact(DisplayName = "Constructor uses disabled defaults")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCreatedUsesDisabledDefaults()
    {
        // Arrange
        var options = new PdfOptions();

        // Assert
        options.OpenAfterGeneration.Should().BeFalse();
    }
}
