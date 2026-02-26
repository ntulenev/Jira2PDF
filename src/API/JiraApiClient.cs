using System.Globalization;
using System.Net;

using JiraReport.Abstractions;
using JiraReport.Models;
using JiraReport.Models.Configuration;
using JiraReport.Transport.Models;

using Microsoft.Extensions.Options;

namespace JiraReport.API;

/// <summary>
/// Jira API client implementation for issue search operations.
/// </summary>
internal sealed class JiraApiClient : IJiraApiClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JiraApiClient"/> class.
    /// </summary>
    /// <param name="transport">HTTP transport abstraction.</param>
    /// <param name="options">Application settings options.</param>
    public JiraApiClient(IJiraTransport transport, IOptions<AppSettings> options)
    {
        ArgumentNullException.ThrowIfNull(transport);
        ArgumentNullException.ThrowIfNull(options);

        _transport = transport;
        _settings = options.Value;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<JiraIssue>> SearchIssuesAsync(string jql, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jql);

        var pageSize = Math.Clamp(_settings.MaxResultsPerPage, 1, 100);

        try
        {
            return await SearchWithPageTokenAsync(jql, pageSize, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return await SearchWithStartAtAsync(jql, pageSize, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<IReadOnlyList<JiraIssue>> SearchWithPageTokenAsync(
        string jql,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var issues = new List<JiraIssue>();
        string? nextPageToken = null;

        while (true)
        {
            var searchUrl =
                $"rest/api/3/search/jql?jql={Uri.EscapeDataString(jql)}&fields={ISSUE_FIELDS}&maxResults={pageSize}";
            if (!string.IsNullOrWhiteSpace(nextPageToken))
            {
                searchUrl += $"&nextPageToken={Uri.EscapeDataString(nextPageToken)}";
            }

            var page = await GetSearchPageAsync(searchUrl, cancellationToken).ConfigureAwait(false);
            issues.AddRange(MapIssues(page));

            nextPageToken = page.NextPageToken;
            if (page.Issues.Count == 0 || page.IsLast || string.IsNullOrWhiteSpace(nextPageToken))
            {
                break;
            }
        }

        return [.. issues
            .DistinctBy(static issue => issue.Key, StringComparer.OrdinalIgnoreCase)
            .OrderBy(static issue => issue.Key, StringComparer.OrdinalIgnoreCase)];
    }

    private async Task<IReadOnlyList<JiraIssue>> SearchWithStartAtAsync(
        string jql,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var issues = new List<JiraIssue>();
        var startAt = 0;

        while (true)
        {
            var searchUrl =
                $"rest/api/3/search?jql={Uri.EscapeDataString(jql)}&fields={ISSUE_FIELDS}&startAt={startAt}&maxResults={pageSize}";

            var page = await GetSearchPageAsync(searchUrl, cancellationToken).ConfigureAwait(false);
            issues.AddRange(MapIssues(page));

            if (page.Issues.Count == 0)
            {
                break;
            }

            startAt += page.Issues.Count;
            var total = page.Total > 0 ? page.Total : startAt;
            if (startAt >= total)
            {
                break;
            }
        }

        return [.. issues
            .DistinctBy(static issue => issue.Key, StringComparer.OrdinalIgnoreCase)
            .OrderBy(static issue => issue.Key, StringComparer.OrdinalIgnoreCase)];
    }

    private async Task<JiraSearchResponse> GetSearchPageAsync(string searchUrl, CancellationToken cancellationToken)
    {
        var page = await _transport
            .GetAsync<JiraSearchResponse>(new Uri(searchUrl, UriKind.Relative), cancellationToken)
            .ConfigureAwait(false);

        return page ?? throw new InvalidOperationException("Jira search response is empty.");
    }

    private static IReadOnlyList<JiraIssue> MapIssues(JiraSearchResponse page)
    {
        if (page.Issues.Count == 0)
        {
            return [];
        }

        return [.. page.Issues
            .Where(static issue => !string.IsNullOrWhiteSpace(issue.Key))
            .Select(static issue =>
            {
                var fields = issue.Fields;
                return new JiraIssue(
                    issue.Key!.Trim(),
                    string.IsNullOrWhiteSpace(fields?.Summary) ? "(no summary)" : fields.Summary.Trim(),
                    string.IsNullOrWhiteSpace(fields?.Status?.Name) ? "Unknown" : fields.Status.Name.Trim(),
                    string.IsNullOrWhiteSpace(fields?.IssueType?.Name) ? "Unknown" : fields.IssueType.Name.Trim(),
                    string.IsNullOrWhiteSpace(fields?.Assignee?.DisplayName) ? "Unassigned" : fields.Assignee.DisplayName.Trim(),
                    ParseDateTimeOffset(fields?.Created),
                    ParseDateTimeOffset(fields?.Updated));
            })];
    }

    private static DateTimeOffset? ParseDateTimeOffset(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        return DateTimeOffset.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    private const string ISSUE_FIELDS = "key,summary,status,issuetype,assignee,created,updated";
    private readonly IJiraTransport _transport;
    private readonly AppSettings _settings;
}
