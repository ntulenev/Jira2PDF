using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Abstractions;

/// <summary>
/// Defines CSV writing for prepared report data.
/// </summary>
internal interface ICsvReportWriter
{
    /// <summary>
    /// Writes report issue rows into a CSV file.
    /// </summary>
    /// <param name="report">Prepared report model.</param>
    /// <param name="outputPath">Resolved CSV output path.</param>
    /// <param name="outputColumns">Selected output columns.</param>
    /// <param name="displayHeaders">Whether to include header row.</param>
    /// <returns>Asynchronous operation task.</returns>
    Task WriteReportAsync(
        JiraJqlReport report,
        CsvFilePath outputPath,
        IReadOnlyList<OutputColumn> outputColumns,
        bool displayHeaders);
}
