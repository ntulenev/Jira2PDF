using JiraReport.Models;
using JiraReport.Transport.Models;

namespace JiraReport.Abstractions;

/// <summary>
/// Maps Jira search DTO pages into domain issue models.
/// </summary>
internal interface IIssueMapper
{
    /// <summary>
    /// Maps Jira search page issues into report issue models.
    /// </summary>
    /// <param name="page">Search page DTO.</param>
    /// <param name="aliasesByApiField">Configured aliases grouped by API field key.</param>
    /// <returns>Mapped issue list for the page.</returns>
    IReadOnlyList<JiraIssue> MapIssues(
        JiraSearchResponse page,
        IReadOnlyDictionary<string, IReadOnlyList<string>> aliasesByApiField);
}
