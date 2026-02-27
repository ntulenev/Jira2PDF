using System.Net;

using JiraReport.Abstractions;
using JiraReport.Models;
using JiraReport.Models.Configuration;
using JiraReport.Models.ValueObjects;
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
    /// <param name="issueMapper">Issue mapper.</param>
    public JiraApiClient(IJiraTransport transport, IOptions<AppSettings> options, IIssueMapper issueMapper)
    {
        ArgumentNullException.ThrowIfNull(transport);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(issueMapper);

        _transport = transport;
        _settings = options.Value;
        _issueMapper = issueMapper;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<JiraIssue>> SearchIssuesAsync(
        JqlQuery jql,
        IReadOnlyList<IssueFieldName> issueFields,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(issueFields);

        var pageSize = Math.Clamp(_settings.MaxResultsPerPage, 1, 100);
        var jqlText = jql.Value;
        var resolvedFields = await ResolveRequestedFieldsAsync(issueFields, cancellationToken).ConfigureAwait(false);
        var requestedFieldsCsv = BuildRequestedFieldsCsv(resolvedFields);
        var aliasesByApiField = BuildAliasesByApiField(resolvedFields);

        try
        {
            return await SearchWithPageTokenAsync(
                    jqlText,
                    requestedFieldsCsv,
                    aliasesByApiField,
                    pageSize,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return await SearchWithStartAtAsync(
                    jqlText,
                    requestedFieldsCsv,
                    aliasesByApiField,
                    pageSize,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task<IReadOnlyList<JiraIssue>> SearchWithPageTokenAsync(
        string jql,
        string requestedFieldsCsv,
        IReadOnlyDictionary<string, IReadOnlyList<string>> aliasesByApiField,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var issues = new List<JiraIssue>();
        string? nextPageToken = null;

        while (true)
        {
            var searchUrl =
                $"rest/api/3/search/jql?jql={Uri.EscapeDataString(jql)}&fields={Uri.EscapeDataString(requestedFieldsCsv)}" +
                $"&maxResults={pageSize}";
            if (!string.IsNullOrWhiteSpace(nextPageToken))
            {
                searchUrl += $"&nextPageToken={Uri.EscapeDataString(nextPageToken)}";
            }

            var page = await GetSearchPageAsync(searchUrl, cancellationToken).ConfigureAwait(false);
            issues.AddRange(_issueMapper.MapIssues(page, aliasesByApiField));

            nextPageToken = page.NextPageToken;
            if (page.Issues.Count == 0 || page.IsLast || string.IsNullOrWhiteSpace(nextPageToken))
            {
                break;
            }
        }

        return [.. issues
            .DistinctBy(static issue => issue.Key.Value, StringComparer.OrdinalIgnoreCase)
            .OrderBy(static issue => issue.Key.Value, StringComparer.OrdinalIgnoreCase)];
    }

    private async Task<IReadOnlyList<JiraIssue>> SearchWithStartAtAsync(
        string jql,
        string requestedFieldsCsv,
        IReadOnlyDictionary<string, IReadOnlyList<string>> aliasesByApiField,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var issues = new List<JiraIssue>();
        var startAt = 0;

        while (true)
        {
            var searchUrl =
                $"rest/api/3/search?jql={Uri.EscapeDataString(jql)}&fields={Uri.EscapeDataString(requestedFieldsCsv)}" +
                $"&startAt={startAt}&maxResults={pageSize}";

            var page = await GetSearchPageAsync(searchUrl, cancellationToken).ConfigureAwait(false);
            issues.AddRange(_issueMapper.MapIssues(page, aliasesByApiField));

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
            .DistinctBy(static issue => issue.Key.Value, StringComparer.OrdinalIgnoreCase)
            .OrderBy(static issue => issue.Key.Value, StringComparer.OrdinalIgnoreCase)];
    }

    private async Task<JiraSearchResponse> GetSearchPageAsync(string searchUrl, CancellationToken cancellationToken)
    {
        var page = await _transport
            .GetAsync<JiraSearchResponse>(new Uri(searchUrl, UriKind.Relative), cancellationToken)
            .ConfigureAwait(false);

        return page ?? throw new InvalidOperationException("Jira search response is empty.");
    }

    private static string NormalizeFieldKey(string fieldKey) => fieldKey.Trim();

    private async Task<IReadOnlyList<ResolvedRequestedField>> ResolveRequestedFieldsAsync(
        IReadOnlyList<IssueFieldName> issueFields,
        CancellationToken cancellationToken)
    {
        var fieldAliases = await GetFieldAliasesAsync(cancellationToken).ConfigureAwait(false);
        var resolvedFields = new List<ResolvedRequestedField>();
        var unresolvedFields = new List<string>();
        var seenConfiguredFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawField in issueFields)
        {
            if (string.IsNullOrWhiteSpace(rawField.Value))
            {
                continue;
            }

            var configuredField = NormalizeFieldKey(rawField.Value);
            if (!seenConfiguredFields.Add(configuredField))
            {
                continue;
            }

            if (string.Equals(configuredField, "key", StringComparison.OrdinalIgnoreCase))
            {
                resolvedFields.Add(new ResolvedRequestedField(configuredField, "key"));
                continue;
            }

            if (TryResolveApiField(configuredField, fieldAliases, out var apiField))
            {
                resolvedFields.Add(new ResolvedRequestedField(configuredField, apiField));
                continue;
            }

            unresolvedFields.Add(configuredField);
        }

        if (unresolvedFields.Count > 0)
        {
            throw new InvalidOperationException(
                "Unable to resolve Jira field names: " +
                string.Join(", ", unresolvedFields.Select(static field => $"'{field}'")) +
                ". Use field names as in JQL or field keys like customfield_12345.");
        }

        return resolvedFields;
    }

    private async Task<IReadOnlyDictionary<string, string>> GetFieldAliasesAsync(CancellationToken cancellationToken)
    {
        if (_fieldAliasesByName is not null)
        {
            return _fieldAliasesByName;
        }

        var fields = await _transport
            .GetAsync<List<JiraFieldDefinitionResponse>>(new Uri(FIELD_CATALOG_URL, UriKind.Relative), cancellationToken)
            .ConfigureAwait(false) ?? [];
        var aliases = BuildFieldAliasLookup(fields);
        _fieldAliasesByName = aliases;
        return _fieldAliasesByName;
    }

    private static bool TryResolveApiField(
        string configuredField,
        IReadOnlyDictionary<string, string> aliases,
        out string apiField)
    {
        if (aliases.TryGetValue(configuredField, out var configuredApiField) &&
            !string.IsNullOrWhiteSpace(configuredApiField))
        {
            apiField = configuredApiField;
            return true;
        }

        var simplified = SimplifyFieldAlias(configuredField);
        if (!string.Equals(simplified, configuredField, StringComparison.OrdinalIgnoreCase) &&
            aliases.TryGetValue(simplified, out var simplifiedApiField) &&
            !string.IsNullOrWhiteSpace(simplifiedApiField))
        {
            apiField = simplifiedApiField;
            return true;
        }

        apiField = string.Empty;
        return false;
    }

    private static Dictionary<string, string> BuildFieldAliasLookup(
        IReadOnlyList<JiraFieldDefinitionResponse> fields)
    {
        var aliases = new Dictionary<string, AliasResolution>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in fields)
        {
            var apiField = !string.IsNullOrWhiteSpace(field.Key)
                ? NormalizeFieldKey(field.Key)
                : NormalizeFieldKey(field.Id ?? string.Empty);
            if (string.IsNullOrWhiteSpace(apiField))
            {
                continue;
            }

            var isSystemField = !apiField.StartsWith("customfield_", StringComparison.OrdinalIgnoreCase);

            // Exact id/key aliases should win over display names when collisions happen.
            AddFieldAlias(aliases, field.Id, apiField, priority: 3, isSystemField);
            AddFieldAlias(aliases, field.Key, apiField, priority: 3, isSystemField);
            AddFieldAlias(aliases, field.Name, apiField, priority: 1, isSystemField);

            foreach (var clauseName in field.ClauseNames)
            {
                AddFieldAlias(aliases, clauseName, apiField, priority: 2, isSystemField);
            }
        }

        return aliases.ToDictionary(
            static pair => pair.Key,
            static pair => pair.Value.ApiField,
            StringComparer.OrdinalIgnoreCase);
    }

    private static void AddFieldAlias(
        Dictionary<string, AliasResolution> aliases,
        string? alias,
        string apiField,
        int priority,
        bool isSystemField)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            return;
        }

        var normalizedAlias = NormalizeFieldKey(alias);
        UpsertAlias(aliases, normalizedAlias, apiField, priority, isSystemField);

        var simplifiedAlias = SimplifyFieldAlias(normalizedAlias);
        if (!string.Equals(simplifiedAlias, normalizedAlias, StringComparison.OrdinalIgnoreCase))
        {
            UpsertAlias(aliases, simplifiedAlias, apiField, priority, isSystemField);
        }
    }

    private static void UpsertAlias(
        Dictionary<string, AliasResolution> aliases,
        string alias,
        string apiField,
        int priority,
        bool isSystemField)
    {
        var candidate = new AliasResolution(apiField, priority, isSystemField);
        if (!aliases.TryGetValue(alias, out var current))
        {
            aliases[alias] = candidate;
            return;
        }

        if (priority > current.Priority)
        {
            aliases[alias] = candidate;
            return;
        }

        if (priority == current.Priority && isSystemField && !current.IsSystemField)
        {
            aliases[alias] = candidate;
        }
    }

    private static string SimplifyFieldAlias(string fieldAlias)
    {
        var value = NormalizeFieldKey(fieldAlias);
        if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
        {
            value = value[1..^1].Trim();
        }

        value = string.Join(" ", value.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        var bracketIndex = value.AsSpan().IndexOf('[');
        if (bracketIndex > 0 && value.Length > 0 && value[^1] == ']')
        {
            value = value[..bracketIndex].TrimEnd();
        }

        return value;
    }

    private static Dictionary<string, IReadOnlyList<string>> BuildAliasesByApiField(
        IReadOnlyList<ResolvedRequestedField> resolvedFields)
    {
        var aliasesByApiField = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in resolvedFields)
        {
            if (string.Equals(field.ApiField, "key", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!aliasesByApiField.TryGetValue(field.ApiField, out var aliases))
            {
                aliases = [];
                aliasesByApiField[field.ApiField] = aliases;
            }

            if (!aliases.Contains(field.ConfiguredField, StringComparer.OrdinalIgnoreCase))
            {
                aliases.Add(field.ConfiguredField);
            }
        }

        return aliasesByApiField.ToDictionary(
            static pair => pair.Key,
            static pair => (IReadOnlyList<string>)pair.Value,
            StringComparer.OrdinalIgnoreCase);
    }

    private static string BuildRequestedFieldsCsv(IReadOnlyList<ResolvedRequestedField> resolvedFields)
    {
        var requestedFields = new List<string>();
        var seenFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in resolvedFields)
        {
            if (string.Equals(field.ApiField, "key", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (seenFields.Add(field.ApiField))
            {
                requestedFields.Add(field.ApiField);
            }
        }

        if (requestedFields.Count == 0)
        {
            requestedFields.AddRange(_defaultIssueFields);
        }

        return string.Join(",", requestedFields);
    }

    private static readonly IReadOnlyList<string> _defaultIssueFields =
        ["summary", "status", "issuetype", "assignee", "created", "updated"];
    private const string FIELD_CATALOG_URL = "rest/api/3/field";
    private IReadOnlyDictionary<string, string>? _fieldAliasesByName;
    private sealed record ResolvedRequestedField(string ConfiguredField, string ApiField);
    private sealed record AliasResolution(string ApiField, int Priority, bool IsSystemField);
    private readonly IIssueMapper _issueMapper;
    private readonly IJiraTransport _transport;
    private readonly AppSettings _settings;
}
