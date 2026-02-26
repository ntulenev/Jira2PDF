using JiraReport.Models.ValueObjects;

namespace JiraReport.Models.Configuration;

internal sealed record AppSettings(
    JiraBaseUrl BaseUrl,
    JiraEmail Email,
    JiraApiToken ApiToken,
    int MaxResultsPerPage,
    int RetryCount,
    string DefaultPdfPath,
    IReadOnlyList<ReportConfig> Reports);
