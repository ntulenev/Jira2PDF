using JiraReport.Models.ValueObjects;

namespace JiraReport.Models.Configuration;

/// <summary>
/// Represents validated application settings.
/// </summary>
/// <param name="BaseUrl">Jira base URL.</param>
/// <param name="Email">Jira user email.</param>
/// <param name="ApiToken">Jira API token.</param>
/// <param name="MaxResultsPerPage">Maximum page size for Jira search requests.</param>
/// <param name="RetryCount">Retry count for transient failures.</param>
/// <param name="Reports">Configured named reports.</param>
/// <param name="Pdf">PDF export settings.</param>
/// <param name="Csv">CSV export settings.</param>
internal sealed record AppSettings(
    JiraBaseUrl BaseUrl,
    JiraEmail Email,
    JiraApiToken ApiToken,
    int MaxResultsPerPage,
    int RetryCount,
    IReadOnlyList<ReportConfig> Reports,
    PdfSettings Pdf,
    CsvSettings Csv);
