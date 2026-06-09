using JiraReport.Models;
using JiraReport.Models.ValueObjects;

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
    /// <param name="computedFields">Optional computed fields by configured field key or name.</param>
    /// <param name="fieldValueConverters">Optional field value converters by configured field key or name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Issues matching the query.</returns>
    Task<IReadOnlyList<JiraIssue>> SearchIssuesAsync(
        JqlQuery jql,
        IReadOnlyList<IssueFieldName> issueFields,
        IReadOnlyDictionary<string, ComputedFieldConfig>? computedFields,
        IReadOnlyDictionary<string, FieldValueConverterConfig>? fieldValueConverters,
        CancellationToken cancellationToken);

    /// <summary>Loads status transition histories for issues.</summary>
    Task<IReadOnlyList<IssueFlow>> LoadIssueFlowsAsync(
        IReadOnlyList<JiraIssue> issues,
        CancellationToken cancellationToken);
}
