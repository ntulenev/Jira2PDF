using System.ComponentModel.DataAnnotations;

namespace JiraReport.Models.Configuration;

/// <summary>
/// Represents raw configuration values from the <c>Jira</c> section.
/// </summary>
internal sealed class JiraOptions
{
    /// <summary>
    /// Gets Jira base URL.
    /// </summary>
    public Uri? BaseUrl { get; init; }

    /// <summary>
    /// Gets Jira user email.
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Gets Jira API token.
    /// </summary>
    public string? ApiToken { get; init; }

    /// <summary>
    /// Gets maximum page size for Jira search calls.
    /// </summary>
    [Range(1, 100)]
    public int MaxResultsPerPage { get; init; } = 100;

    /// <summary>
    /// Gets retry count for transient request failures.
    /// </summary>
    [Range(0, 10)]
    public int RetryCount { get; init; } = 3;

    /// <summary>
    /// Gets named report configurations.
    /// </summary>
    public IReadOnlyList<ReportConfigOptions>? Reports { get; init; }
}
