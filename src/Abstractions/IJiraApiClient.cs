using JiraReport.Models;

namespace JiraReport.Abstractions;

internal interface IJiraApiClient
{
    Task<IReadOnlyList<JiraIssue>> SearchIssuesAsync(string jql, CancellationToken cancellationToken);
}
