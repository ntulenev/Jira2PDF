using JiraReport.Models;

namespace JiraReport.Abstractions;

/// <summary>
/// Defines Jira API read operations used by the application.
/// </summary>
internal interface IJiraApiClient
{
    /// <summary>
    /// Searches Jira issues by JQL query.
    /// </summary>
    /// <param name="jql">JQL expression.</param>
    /// <param name="issueFields">Requested Jira field keys.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Issues matching the query.</returns>
    Task<IReadOnlyList<JiraIssue>> SearchIssuesAsync(
        string jql,
        IReadOnlyList<string> issueFields,
        CancellationToken cancellationToken);
}
