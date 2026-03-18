namespace JiraReport.Models.Configuration;

/// <summary>
/// Represents validated CSV export settings.
/// </summary>
/// <param name="Enabled">Whether CSV export is enabled.</param>
/// <param name="DisplayHeaders">Whether CSV output should include header row.</param>
internal sealed record CsvSettings(bool Enabled, bool DisplayHeaders);
