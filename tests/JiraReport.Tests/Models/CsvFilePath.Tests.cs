using FluentAssertions;

using JiraReport.Models.ValueObjects;

namespace JiraReport.Tests.Models;

public sealed class CsvFilePathTests
{
    [Fact(DisplayName = "FromPdfPath appends raw suffix before CSV extension")]
    [Trait("Category", "Unit")]
    public void FromPdfPathWhenCalledUsesPdfFileNameWithRawSuffix()
    {
        // Arrange
        var pdfPath = new PdfFilePath(@"C:\reports\CHG_Report_20260318_081500.pdf");

        // Act
        var csvPath = CsvFilePath.FromPdfPath(pdfPath);

        // Assert
        csvPath.Value.Should().Be(@"C:\reports\CHG_Report_20260318_081500_raw.csv");
    }
}
