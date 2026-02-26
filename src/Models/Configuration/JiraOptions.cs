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
    [Required]
    public required Uri BaseUrl { get; init; }

    /// <summary>
    /// Gets Jira user email.
    /// </summary>
    [Required]
    public required string Email { get; init; }

    /// <summary>
    /// Gets Jira API token.
    /// </summary>
    [Required]
    public required string ApiToken { get; init; }

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
    [Required]
    [MinLength(1)]
    public required IReadOnlyList<ReportConfigOptions> Reports { get; init; }
}
