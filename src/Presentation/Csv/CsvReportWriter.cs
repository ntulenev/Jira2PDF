using System.Text;

using JiraReport.Abstractions;
using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Presentation.Csv;

/// <summary>
/// Writes report issue rows into CSV files.
/// </summary>
internal sealed class CsvReportWriter : ICsvReportWriter
{
    /// <inheritdoc />
    public void WriteReport(
        JiraJqlReport report,
        CsvFilePath outputPath,
        IReadOnlyList<OutputColumn> outputColumns,
        bool displayHeaders)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(outputColumns);

        var directoryPath = Path.GetDirectoryName(outputPath.Value);
        if (!string.IsNullOrWhiteSpace(directoryPath))
        {
            _ = Directory.CreateDirectory(directoryPath);
        }

        using var writer = new StreamWriter(outputPath.Value, append: false, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        if (displayHeaders)
        {
            writer.WriteLine(string.Join(",", outputColumns.Select(static column => Escape(column.Header.Value))));
        }

        foreach (var issue in report.Issues)
        {
            writer.WriteLine(string.Join(",", outputColumns.Select(column => Escape(column.Selector(issue).Value))));
        }
    }

    private static string Escape(string value)
    {
        var normalizedValue = value.Replace("\"", "\"\"", StringComparison.Ordinal);
        return normalizedValue.IndexOfAny([',', '"', '\r', '\n']) >= 0
            ? $"\"{normalizedValue}\""
            : normalizedValue;
    }
}
