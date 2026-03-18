namespace JiraReport.Models.Configuration;

/// <summary>
/// Represents raw CSV export configuration values from the root <c>csv</c> section.
/// </summary>
internal sealed class CsvOptions
{
    /// <summary>
    /// Gets a value indicating whether CSV export is enabled.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Gets a value indicating whether CSV output should include header row.
    /// </summary>
    public bool DisplayHeaders { get; init; }
}
