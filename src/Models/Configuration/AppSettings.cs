using JiraReport.Models.ValueObjects;

namespace JiraReport.Models.Configuration;

/// <summary>
/// Represents validated application settings.
/// </summary>
internal sealed record AppSettings
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettings"/> record.
    /// </summary>
    public AppSettings(
        JiraBaseUrl baseUrl,
        JiraEmail email,
        JiraApiToken apiToken,
        int maxResultsPerPage,
        int retryCount,
        IReadOnlyList<ReportConfig> reports,
        PdfSettings pdf,
        CsvSettings csv,
        UiSettings? ui = null)
    {
        BaseUrl = baseUrl;
        Email = email;
        ApiToken = apiToken;
        MaxResultsPerPage = maxResultsPerPage;
        RetryCount = retryCount;
        Reports = reports;
        Pdf = pdf;
        Csv = csv;
        Ui = ui ?? new UiSettings(15);
    }

    /// <summary>
    /// Gets Jira base URL.
    /// </summary>
    public JiraBaseUrl BaseUrl { get; init; }

    /// <summary>
    /// Gets Jira user email.
    /// </summary>
    public JiraEmail Email { get; init; }

    /// <summary>
    /// Gets Jira API token.
    /// </summary>
    public JiraApiToken ApiToken { get; init; }

    /// <summary>
    /// Gets maximum page size for Jira search requests.
    /// </summary>
    public int MaxResultsPerPage { get; init; }

    /// <summary>
    /// Gets retry count for transient failures.
    /// </summary>
    public int RetryCount { get; init; }

    /// <summary>
    /// Gets configured named reports.
    /// </summary>
    public IReadOnlyList<ReportConfig> Reports { get; init; }

    /// <summary>
    /// Gets PDF export settings.
    /// </summary>
    public PdfSettings Pdf { get; init; }

    /// <summary>
    /// Gets CSV export settings.
    /// </summary>
    public CsvSettings Csv { get; init; }

    /// <summary>
    /// Gets console UI settings.
    /// </summary>
    public UiSettings Ui { get; init; }
}
