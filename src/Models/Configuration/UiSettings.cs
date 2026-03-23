namespace JiraReport.Models.Configuration;

/// <summary>
/// Represents validated console UI settings.
/// </summary>
/// <param name="ReportSelectionPageSize">Number of report items visible in the selection prompt.</param>
internal sealed record UiSettings(int ReportSelectionPageSize);
