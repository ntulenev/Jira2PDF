using System.Net;
using System.Text.Json;

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
        IReadOnlyDictionary<string, ComputedFieldConfig>? computedFields,
        IReadOnlyDictionary<string, FieldValueConverterConfig>? fieldValueConverters,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(issueFields);

        var pageSize = Math.Clamp(_settings.MaxResultsPerPage, 1, 100);
        var jqlText = jql.Value;
        var resolvedFields = await ResolveRequestedFieldsAsync(issueFields, cancellationToken).ConfigureAwait(false);
        var fieldAliases = await GetFieldAliasesAsync(cancellationToken).ConfigureAwait(false);
        var resolvedComputedFields = ResolveComputedFields(resolvedFields, computedFields, fieldAliases);
        var resolvedFieldValueConverters = ResolveFieldValueConverters(
            resolvedFields,
            fieldValueConverters,
            fieldAliases);
        var requestedFieldsCsv = BuildRequestedFieldsCsv(resolvedFields, resolvedComputedFields);
        var aliasesByApiField = BuildAliasesByApiField(resolvedFields);

        try
        {
            return await SearchWithPageTokenAsync(
                    jqlText,
                    requestedFieldsCsv,
                    aliasesByApiField,
                    resolvedFieldValueConverters,
                    resolvedComputedFields,
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
                    resolvedFieldValueConverters,
                    resolvedComputedFields,
                    pageSize,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }

    private async Task<IReadOnlyList<JiraIssue>> SearchWithPageTokenAsync(
        string jql,
        string requestedFieldsCsv,
        IReadOnlyDictionary<string, IReadOnlyList<string>> aliasesByApiField,
        IReadOnlyDictionary<string, FieldValueConverterConfig> convertersByApiField,
        IReadOnlyDictionary<string, ComputedFieldConfig> computedFieldsByApiField,
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
            await ApplyComputedFieldsAsync(page, computedFieldsByApiField, pageSize, cancellationToken)
                .ConfigureAwait(false);
            issues.AddRange(_issueMapper.MapIssues(page, aliasesByApiField, convertersByApiField));

            nextPageToken = page.NextPageToken;
            if (page.Issues.Count == 0 || page.IsLast || string.IsNullOrWhiteSpace(nextPageToken))
            {
                break;
            }
        }

        return issues;
    }

    private async Task<IReadOnlyList<JiraIssue>> SearchWithStartAtAsync(
        string jql,
        string requestedFieldsCsv,
        IReadOnlyDictionary<string, IReadOnlyList<string>> aliasesByApiField,
        IReadOnlyDictionary<string, FieldValueConverterConfig> convertersByApiField,
        IReadOnlyDictionary<string, ComputedFieldConfig> computedFieldsByApiField,
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
            await ApplyComputedFieldsAsync(page, computedFieldsByApiField, pageSize, cancellationToken)
                .ConfigureAwait(false);
            issues.AddRange(_issueMapper.MapIssues(page, aliasesByApiField, convertersByApiField));

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

        return issues;
    }

    private async Task<JiraSearchResponse> GetSearchPageAsync(string searchUrl, CancellationToken cancellationToken)
    {
        var page = await _transport
            .GetAsync<JiraSearchResponse>(new Uri(searchUrl, UriKind.Relative), cancellationToken)
            .ConfigureAwait(false);

        return page ?? throw new InvalidOperationException("Jira search response is empty.");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<IssueFlow>> LoadIssueFlowsAsync(
        IReadOnlyList<JiraIssue> issues,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(issues);
        var byId = issues
            .Where(static issue => !string.IsNullOrWhiteSpace(issue.JiraId))
            .ToDictionary(static issue => issue.JiraId!, StringComparer.OrdinalIgnoreCase);
        if (byId.Count == 0)
        {
            return [];
        }

        var histories = new Dictionary<string, List<JiraBulkHistory>>(StringComparer.OrdinalIgnoreCase);
        foreach (var batch in byId.Keys.Chunk(100))
        {
            string? token = null;
            do
            {
                var response = await _transport.PostAsync<JiraBulkChangelogRequest, JiraBulkChangelogResponse>(
                    new Uri("rest/api/3/changelog/bulkfetch", UriKind.Relative),
                    new JiraBulkChangelogRequest { IssueIdsOrKeys = batch, NextPageToken = token },
                    cancellationToken).ConfigureAwait(false)
                    ?? throw new InvalidOperationException("Jira bulk changelog response is empty.");
                foreach (var item in response.IssueChangeLogs.Where(static item => !string.IsNullOrWhiteSpace(item.IssueId)))
                {
                    if (!histories.TryGetValue(item.IssueId!, out var list))
                    {
                        list = [];
                        histories[item.IssueId!] = list;
                    }
                    list.AddRange(item.ChangeHistories);
                }
                token = response.NextPageToken;
            }
            while (!string.IsNullOrWhiteSpace(token));
        }

        return [.. byId.Select(pair => BuildIssueFlow(pair.Value, histories.GetValueOrDefault(pair.Key, [])))];
    }

    private static IssueFlow BuildIssueFlow(JiraIssue issue, IReadOnlyList<JiraBulkHistory> histories)
    {
        var raw = histories
            .SelectMany(history => history.Items
                .Where(static item => string.Equals(item.Field, "status", StringComparison.OrdinalIgnoreCase) &&
                    !string.IsNullOrWhiteSpace(item.From) && !string.IsNullOrWhiteSpace(item.To))
                .Select(item => (At: ParseTimestamp(history.Created), From: item.From!.Trim(), To: item.To!.Trim())))
            .Where(static item => item.At.HasValue)
            .OrderBy(static item => item.At)
            .ToList();
        var previous = issue.CreatedAt ?? raw.FirstOrDefault().At ?? DateTimeOffset.MinValue;
        var transitions = new List<FlowTransition>(raw.Count);
        foreach (var (At, From, To) in raw)
        {
            var at = At!.Value;
            transitions.Add(new FlowTransition(From, To, at, at > previous ? at - previous : TimeSpan.Zero));
            previous = at;
        }
        return new IssueFlow(issue.Key, transitions);
    }

    private static DateTimeOffset? ParseTimestamp(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.String &&
            DateTimeOffset.TryParse(value.GetString(), out var parsed))
        {
            return parsed;
        }
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var unix))
        {
            return Math.Abs(unix) >= 1_000_000_000_000
                ? DateTimeOffset.FromUnixTimeMilliseconds(unix)
                : DateTimeOffset.FromUnixTimeSeconds(unix);
        }
        return null;
    }

    private async Task ApplyComputedFieldsAsync(
        JiraSearchResponse page,
        IReadOnlyDictionary<string, ComputedFieldConfig> computedFieldsByApiField,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (page.Issues.Count == 0 || computedFieldsByApiField.Count == 0)
        {
            return;
        }

        foreach (var (apiField, computedField) in computedFieldsByApiField)
        {
            if (IsLinkedIssueProgress(computedField))
            {
                await ApplyLinkedIssueProgressAsync(page, apiField, computedField, pageSize, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

    private async Task ApplyLinkedIssueProgressAsync(
        JiraSearchResponse page,
        string apiField,
        ComputedFieldConfig computedField,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var linkedIssuesByIssueKey = CollectLinkedIssuesByIssueKey(page, computedField.LinkType);
        var linkedIssueKeys = linkedIssuesByIssueKey.Values
            .SelectMany(static issues => issues)
            .Select(static issue => issue.Key)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var childrenByParentKey = string.Equals(computedField.Mode, "Default", StringComparison.OrdinalIgnoreCase)
            ? await LoadChildIssueStatusesAsync(linkedIssueKeys, computedField.ChildJqlTemplate, pageSize, cancellationToken)
                .ConfigureAwait(false)
            : new Dictionary<string, List<LinkedIssueStatus>>(StringComparer.OrdinalIgnoreCase);

        foreach (var issue in page.Issues)
        {
            if (string.IsNullOrWhiteSpace(issue.Key) ||
                issue.Fields is null)
            {
                continue;
            }

            if (!linkedIssuesByIssueKey.TryGetValue(issue.Key.Trim(), out var linkedIssues))
            {
                linkedIssues = [];
            }

            var progress = CalculateLinkedIssueProgress(linkedIssues, childrenByParentKey, computedField);
            issue.Fields.Values[apiField] = JsonSerializer.SerializeToElement(FormatComputedValue(progress, computedField.Format));
        }
    }

    private async Task<Dictionary<string, List<LinkedIssueStatus>>> LoadChildIssueStatusesAsync(
        List<string> parentKeys,
        string childJqlTemplate,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var childrenByParentKey = new Dictionary<string, List<LinkedIssueStatus>>(StringComparer.OrdinalIgnoreCase);
        if (parentKeys.Count == 0 || !childJqlTemplate.Contains(KEYS_PLACEHOLDER, StringComparison.Ordinal))
        {
            return childrenByParentKey;
        }

        var jql = childJqlTemplate.Replace(KEYS_PLACEHOLDER, string.Join(", ", parentKeys), StringComparison.Ordinal);
        string? nextPageToken = null;

        while (true)
        {
            var searchUrl =
                $"rest/api/3/search/jql?jql={Uri.EscapeDataString(jql)}&fields={Uri.EscapeDataString("status,parent")}" +
                $"&maxResults={pageSize}";
            if (!string.IsNullOrWhiteSpace(nextPageToken))
            {
                searchUrl += $"&nextPageToken={Uri.EscapeDataString(nextPageToken)}";
            }

            var page = await GetSearchPageAsync(searchUrl, cancellationToken).ConfigureAwait(false);
            foreach (var issue in page.Issues)
            {
                if (issue.Fields?.Values is not { Count: > 0 } fields ||
                    !TryExtractParentKey(fields, out var parentKey) ||
                    !TryExtractStatus(fields, out var status))
                {
                    continue;
                }

                if (!childrenByParentKey.TryGetValue(parentKey, out var children))
                {
                    children = [];
                    childrenByParentKey[parentKey] = children;
                }

                children.Add(status);
            }

            nextPageToken = page.NextPageToken;
            if (page.Issues.Count == 0 || page.IsLast || string.IsNullOrWhiteSpace(nextPageToken))
            {
                break;
            }
        }

        return childrenByParentKey;
    }

    private static string NormalizeFieldKey(string fieldKey) => fieldKey.Trim();

    private static Dictionary<string, List<LinkedIssueStatus>> CollectLinkedIssuesByIssueKey(
        JiraSearchResponse page,
        string linkType)
    {
        var linkedIssuesByIssueKey = new Dictionary<string, List<LinkedIssueStatus>>(StringComparer.OrdinalIgnoreCase);
        foreach (var issue in page.Issues)
        {
            if (string.IsNullOrWhiteSpace(issue.Key) ||
                issue.Fields?.Values is not { Count: > 0 } fields ||
                !fields.TryGetValue(ISSUE_LINKS_API_FIELD, out var issueLinks) ||
                issueLinks.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var link in issueLinks.EnumerateArray())
            {
                if (!IsExpectedLinkType(link, linkType) ||
                    !TryExtractLinkedIssueStatus(link, out var linkedIssue))
                {
                    continue;
                }

                var issueKey = issue.Key.Trim();
                if (!linkedIssuesByIssueKey.TryGetValue(issueKey, out var linkedIssues))
                {
                    linkedIssues = [];
                    linkedIssuesByIssueKey[issueKey] = linkedIssues;
                }

                linkedIssues.Add(linkedIssue);
            }
        }

        return linkedIssuesByIssueKey;
    }

    private static ComputedProgress CalculateLinkedIssueProgress(
        IReadOnlyList<LinkedIssueStatus> linkedIssues,
        IReadOnlyDictionary<string, List<LinkedIssueStatus>> childrenByParentKey,
        ComputedFieldConfig computedField)
    {
        var total = 0;
        var done = 0;
        var doneCategories = computedField.DoneStatusCategories.Count > 0
            ? computedField.DoneStatusCategories
            : _defaultDoneStatusCategories;

        foreach (var linkedIssue in linkedIssues)
        {
            if (!string.Equals(computedField.Mode, "Default", StringComparison.OrdinalIgnoreCase))
            {
                total++;
                if (IsDone(linkedIssue, doneCategories))
                {
                    done++;
                }

                continue;
            }

            if (IsDone(linkedIssue, doneCategories))
            {
                var doneIssueCount = childrenByParentKey.TryGetValue(linkedIssue.Key, out var doneChildren) && doneChildren.Count > 0
                    ? doneChildren.Count
                    : 1;
                total += doneIssueCount;
                done += doneIssueCount;
                continue;
            }

            if (childrenByParentKey.TryGetValue(linkedIssue.Key, out var children) && children.Count > 0)
            {
                total += children.Count;
                done += children.Count(child => IsDone(child, doneCategories));
                continue;
            }

            total++;
        }

        return new ComputedProgress(done, total);
    }

    private static bool IsDone(LinkedIssueStatus issue, IReadOnlyList<string> doneCategories)
        => doneCategories.Contains(issue.StatusCategoryKey, StringComparer.OrdinalIgnoreCase);

    private static string FormatComputedValue(ComputedProgress progress, string format)
    {
        var percentDone = progress.Total == 0
            ? 0
            : progress.Done * 100d / progress.Total;
        var roundedPercentDone = Math.Round(percentDone, MidpointRounding.AwayFromZero);
        return format
            .Replace("{PercentDone:0}", roundedPercentDone.ToString("0", System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{Done}", progress.Done.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{Total}", progress.Total.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    private static bool IsExpectedLinkType(JsonElement link, string linkType)
        => link.TryGetProperty("type", out var type) &&
            type.TryGetProperty("name", out var name) &&
            string.Equals(name.GetString(), linkType, StringComparison.OrdinalIgnoreCase);

    private static bool TryExtractLinkedIssueStatus(JsonElement link, out LinkedIssueStatus linkedIssue)
    {
        linkedIssue = default;
        if (!TryGetLinkedIssue(link, out var issue) ||
            !issue.TryGetProperty("key", out var keyProperty) ||
            !issue.TryGetProperty("fields", out var fields) ||
            !TryExtractStatusFromFields(fields, out var status))
        {
            return false;
        }

        var key = keyProperty.GetString();
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        linkedIssue = new LinkedIssueStatus(key.Trim(), status.StatusName, status.StatusCategoryKey);
        return true;
    }

    private static bool TryGetLinkedIssue(JsonElement link, out JsonElement issue)
    {
        if (link.TryGetProperty("inwardIssue", out issue))
        {
            return true;
        }

        return link.TryGetProperty("outwardIssue", out issue);
    }

    private static bool TryExtractParentKey(
        Dictionary<string, JsonElement> fields,
        out string parentKey)
    {
        parentKey = string.Empty;
        if (!fields.TryGetValue("parent", out var parent) ||
            parent.ValueKind != JsonValueKind.Object ||
            !parent.TryGetProperty("key", out var key))
        {
            return false;
        }

        parentKey = key.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(parentKey);
    }

    private static bool TryExtractStatus(
        Dictionary<string, JsonElement> fields,
        out LinkedIssueStatus status)
    {
        status = default;
        if (!fields.TryGetValue("status", out var statusElement))
        {
            return false;
        }

        return TryExtractStatus(statusElement, out status);
    }

    private static bool TryExtractStatusFromFields(JsonElement fields, out LinkedIssueStatus status)
    {
        status = default;
        if (!fields.TryGetProperty("status", out var statusElement))
        {
            return false;
        }

        return TryExtractStatus(statusElement, out status);
    }

    private static bool TryExtractStatus(JsonElement statusElement, out LinkedIssueStatus status)
    {
        status = default;
        var statusName = statusElement.TryGetProperty("name", out var name)
            ? name.GetString() ?? string.Empty
            : string.Empty;
        var statusCategoryKey = statusElement.TryGetProperty("statusCategory", out var statusCategory) &&
            statusCategory.TryGetProperty("key", out var key)
                ? key.GetString() ?? string.Empty
                : string.Empty;

        if (string.IsNullOrWhiteSpace(statusName) && string.IsNullOrWhiteSpace(statusCategoryKey))
        {
            return false;
        }

        status = new LinkedIssueStatus(string.Empty, statusName, statusCategoryKey);
        return true;
    }

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

            if (TryResolveApiFields(configuredField, fieldAliases, out var apiFields))
            {
                foreach (var apiField in apiFields)
                {
                    resolvedFields.Add(new ResolvedRequestedField(configuredField, apiField));
                }

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

    private async Task<IReadOnlyDictionary<string, IReadOnlyList<string>>> GetFieldAliasesAsync(CancellationToken cancellationToken)
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

    private static bool TryResolveApiFields(
        string configuredField,
        IReadOnlyDictionary<string, IReadOnlyList<string>> aliases,
        out IReadOnlyList<string> apiFields)
    {
        if (aliases.TryGetValue(configuredField, out var configuredApiFields) &&
            configuredApiFields.Count > 0)
        {
            apiFields = configuredApiFields;
            return true;
        }

        var simplified = SimplifyFieldAlias(configuredField);
        if (!string.Equals(simplified, configuredField, StringComparison.OrdinalIgnoreCase) &&
            aliases.TryGetValue(simplified, out var simplifiedApiFields) &&
            simplifiedApiFields.Count > 0)
        {
            apiFields = simplifiedApiFields;
            return true;
        }

        apiFields = [];
        return false;
    }

    private static Dictionary<string, IReadOnlyList<string>> BuildFieldAliasLookup(
        IReadOnlyList<JiraFieldDefinitionResponse> fields)
    {
        var aliases = new Dictionary<string, List<AliasResolution>>(StringComparer.OrdinalIgnoreCase);

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
            static pair => (IReadOnlyList<string>)[.. pair.Value.Select(static value => value.ApiField)],
            StringComparer.OrdinalIgnoreCase);
    }

    private static void AddFieldAlias(
        Dictionary<string, List<AliasResolution>> aliases,
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
        Dictionary<string, List<AliasResolution>> aliases,
        string alias,
        string apiField,
        int priority,
        bool isSystemField)
    {
        var candidate = new AliasResolution(apiField, priority, isSystemField);
        if (!aliases.TryGetValue(alias, out var current))
        {
            aliases[alias] = [candidate];
            return;
        }

        var currentPriority = current[0].Priority;
        if (priority > currentPriority)
        {
            aliases[alias] = [candidate];
            return;
        }

        if (priority < currentPriority)
        {
            return;
        }

        var currentHasSystemField = current.Any(static value => value.IsSystemField);
        if (isSystemField && !currentHasSystemField)
        {
            aliases[alias] = [candidate];
            return;
        }

        if (!isSystemField && currentHasSystemField)
        {
            return;
        }

        if (current.All(value => !string.Equals(value.ApiField, apiField, StringComparison.OrdinalIgnoreCase)))
        {
            current.Add(candidate);
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

    private static Dictionary<string, ComputedFieldConfig> ResolveComputedFields(
        IReadOnlyList<ResolvedRequestedField> resolvedFields,
        IReadOnlyDictionary<string, ComputedFieldConfig>? computedFields,
        IReadOnlyDictionary<string, IReadOnlyList<string>> fieldAliases)
    {
        var resolvedComputedFields = new Dictionary<string, ComputedFieldConfig>(StringComparer.OrdinalIgnoreCase);
        if (computedFields is null || computedFields.Count == 0)
        {
            return resolvedComputedFields;
        }

        var requestedApiFields = resolvedFields
            .Select(static field => field.ApiField)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (configuredField, computedField) in computedFields)
        {
            if (!IsLinkedIssueProgress(computedField) ||
                !string.Equals(computedField.Metric, "IssueCount", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var normalizedField = NormalizeFieldKey(configuredField);
            var apiFields = TryResolveApiFields(normalizedField, fieldAliases, out var resolvedApiFields)
                ? resolvedApiFields
                : [normalizedField];

            foreach (var apiField in apiFields)
            {
                if (requestedApiFields.Contains(apiField))
                {
                    resolvedComputedFields[apiField] = computedField;
                }
            }
        }

        return resolvedComputedFields;
    }

    private static Dictionary<string, FieldValueConverterConfig> ResolveFieldValueConverters(
        IReadOnlyList<ResolvedRequestedField> resolvedFields,
        IReadOnlyDictionary<string, FieldValueConverterConfig>? fieldValueConverters,
        IReadOnlyDictionary<string, IReadOnlyList<string>> fieldAliases)
    {
        var resolvedConverters = new Dictionary<string, FieldValueConverterConfig>(StringComparer.OrdinalIgnoreCase);
        if (fieldValueConverters is null || fieldValueConverters.Count == 0)
        {
            return resolvedConverters;
        }

        var requestedApiFields = resolvedFields
            .Select(static field => field.ApiField)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var (configuredField, converter) in fieldValueConverters)
        {
            if (!IsJsonPathConverter(converter))
            {
                continue;
            }

            var normalizedField = NormalizeFieldKey(configuredField);
            var apiFields = TryResolveApiFields(normalizedField, fieldAliases, out var resolvedApiFields)
                ? resolvedApiFields
                : [normalizedField];

            foreach (var apiField in apiFields)
            {
                if (requestedApiFields.Contains(apiField))
                {
                    resolvedConverters[apiField] = converter;
                }
            }
        }

        return resolvedConverters;
    }

    private static bool IsLinkedIssueProgress(ComputedFieldConfig computedField)
        => string.Equals(computedField.Type, "LinkedIssueProgress", StringComparison.OrdinalIgnoreCase);

    private static bool IsJsonPathConverter(FieldValueConverterConfig converter)
        => string.Equals(converter.Type, "JsonPath", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(converter.Type, "JsonProperty", StringComparison.OrdinalIgnoreCase);

    private static string BuildRequestedFieldsCsv(
        IReadOnlyList<ResolvedRequestedField> resolvedFields,
        Dictionary<string, ComputedFieldConfig> computedFieldsByApiField)
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

        if (computedFieldsByApiField.Count > 0 && seenFields.Add(ISSUE_LINKS_API_FIELD))
        {
            requestedFields.Add(ISSUE_LINKS_API_FIELD);
        }

        if (requestedFields.Count == 0)
        {
            requestedFields.AddRange(_defaultIssueFields);
        }

        return string.Join(",", requestedFields);
    }

    private static readonly IReadOnlyList<string> _defaultIssueFields =
        ["summary", "status", "issuetype", "assignee", "created", "updated"];
    private static readonly IReadOnlyList<string> _defaultDoneStatusCategories = ["done"];
    private const string ISSUE_LINKS_API_FIELD = "issuelinks";
    private const string KEYS_PLACEHOLDER = "{keys}";
    private const string FIELD_CATALOG_URL = "rest/api/3/field";
    private IReadOnlyDictionary<string, IReadOnlyList<string>>? _fieldAliasesByName;
    private sealed record ResolvedRequestedField(string ConfiguredField, string ApiField);
    private sealed record AliasResolution(string ApiField, int Priority, bool IsSystemField);
    private readonly record struct LinkedIssueStatus(string Key, string StatusName, string StatusCategoryKey);
    private readonly record struct ComputedProgress(int Done, int Total);
    private readonly IIssueMapper _issueMapper;
    private readonly IJiraTransport _transport;
    private readonly AppSettings _settings;
}
