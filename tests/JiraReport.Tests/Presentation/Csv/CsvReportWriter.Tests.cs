using FluentAssertions;

using JiraReport.Models;
using JiraReport.Models.ValueObjects;
using JiraReport.Presentation.Csv;

namespace JiraReport.Tests.Presentation.Csv;

public sealed class CsvReportWriterTests
{
    [Fact(DisplayName = "WriteReport writes headers and escapes values when enabled")]
    [Trait("Category", "Unit")]
    public void WriteReportWhenHeadersAreEnabledWritesHeaderAndEscapedRows()
    {
        // Arrange
        var writer = new CsvReportWriter();
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var outputPath = new CsvFilePath(Path.Combine(tempDirectory, "jira_raw.csv"));
        var report = new JiraJqlReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            new DateTimeOffset(2026, 2, 28, 18, 0, 0, TimeSpan.Zero),
            [
                new JiraIssue(
                    new IssueKey("APP-1"),
                    new Dictionary<IssueKey, FieldValue>
                    {
                        [new IssueKey("summary")] = new FieldValue("Fix \"CSV\", parser")
                    })
            ],
            []);
        var columns = new[]
        {
            new OutputColumn(IssueKey.DefaultKey, new OutputColumnHeader("Key"), static issue => issue.GetFieldValue(IssueKey.DefaultKey)),
            new OutputColumn(new IssueKey("summary"), new OutputColumnHeader("Summary"), static issue => issue.GetFieldValue(new IssueKey("summary")))
        };

        try
        {
            // Act
            writer.WriteReport(report, outputPath, columns, displayHeaders: true);

            // Assert
            var lines = File.ReadAllLines(outputPath.Value);
            lines.Should().ContainInOrder("Key,Summary", "APP-1,\"Fix \"\"CSV\"\", parser\"");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }

    [Fact(DisplayName = "WriteReport omits headers when disabled")]
    [Trait("Category", "Unit")]
    public void WriteReportWhenHeadersAreDisabledOmitsHeaderRow()
    {
        // Arrange
        var writer = new CsvReportWriter();
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var outputPath = new CsvFilePath(Path.Combine(tempDirectory, "jira_raw.csv"));
        var report = new JiraJqlReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            new DateTimeOffset(2026, 2, 28, 18, 0, 0, TimeSpan.Zero),
            [new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue> { [new IssueKey("summary")] = new FieldValue("Implement report") })],
            []);
        var columns = new[]
        {
            new OutputColumn(new IssueKey("summary"), new OutputColumnHeader("Summary"), static issue => issue.GetFieldValue(new IssueKey("summary")))
        };

        try
        {
            // Act
            writer.WriteReport(report, outputPath, columns, displayHeaders: false);

            // Assert
            var lines = File.ReadAllLines(outputPath.Value);
            lines.Should().ContainSingle().Which.Should().Be("Implement report");
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }
}
