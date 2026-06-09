using JiraReport.Abstractions;
using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Logic;

/// <summary>
/// Default domain logic implementation for report preparation.
/// </summary>
internal sealed class JiraLogicService : IJiraLogicService
{
    /// <inheritdoc />
    public IReadOnlyList<OutputColumn> ResolveOutputColumns(
        IReadOnlyList<IssueFieldName>? configuredFields,
        IReadOnlyDictionary<string, string>? configuredFieldAliases = null)
    {
        var fieldKeys = ResolveConfiguredFieldKeys(configuredFields, _defaultOutputOrder);
        var columns = new List<OutputColumn>(fieldKeys.Count);

        foreach (var fieldKey in fieldKeys)
        {
            var key = fieldKey.Value;
            var issueFieldKey = new IssueKey(key);
            var header = TryGetFieldAlias(key, configuredFieldAliases, out var alias)
                ? new OutputColumnHeader(alias)
                : OutputColumnHeader.FromFieldKey(key);
            columns.Add(new OutputColumn(
                issueFieldKey,
                header,
                issue => issue.GetFieldValue(issueFieldKey)));
        }

        return columns;
    }

    /// <inheritdoc />
    public IReadOnlyList<IssueFieldName> ResolveRequestedIssueFields(
        IReadOnlyList<IssueFieldName>? configuredOutputFields,
        IReadOnlyList<IssueFieldName>? configuredCountFields)
    {
        var outputFields = ResolveConfiguredFieldKeys(configuredOutputFields, _defaultOutputOrder);
        var countFields = ResolveCountFieldKeys(configuredCountFields);

        var requestedFields = new List<IssueFieldName>();
        var seenFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in outputFields)
        {
            if (seenFields.Add(field.Value))
            {
                requestedFields.Add(field);
            }
        }

        foreach (var field in countFields)
        {
            if (seenFields.Add(field.Value))
            {
                requestedFields.Add(field);
            }
        }

        return requestedFields;
    }

    /// <inheritdoc />
    public PdfFilePath BuildDefaultPdfPath(PdfReportName reportTitle, DateTimeOffset generatedAt)
        => PdfFilePath.FromReportTitle(reportTitle, generatedAt);

    /// <inheritdoc />
    public JiraJqlReport BuildReport(
        PdfReportName reportTitle,
        ReportName configName,
        JqlQuery jql,
        IReadOnlyList<JiraIssue> issues,
        IReadOnlyList<IssueFieldName>? configuredCountFields,
        IReadOnlyDictionary<string, string>? configuredCountFieldAliases = null)
    {
        ArgumentNullException.ThrowIfNull(issues);

        var countTables = ResolveCountTables(issues, configuredCountFields, configuredCountFieldAliases);
        return new JiraJqlReport(reportTitle, configName, jql, DateTimeOffset.Now, issues, countTables);
    }

    /// <inheritdoc />
    public IReadOnlyList<FlowPathGroup> BuildFlowPathGroups(IReadOnlyList<IssueFlow> flows)
    {
        ArgumentNullException.ThrowIfNull(flows);
        return [.. flows
            .Where(static flow => flow.Transitions.Count > 0)
            .GroupBy(static flow => string.Join(" -> ", flow.Transitions.Select(static transition => transition.From).Append(flow.Transitions[^1].To)),
                StringComparer.OrdinalIgnoreCase)
            .Select(group => new FlowPathGroup(
                group.Key,
                [.. group.Select(static flow => flow.Key).OrderBy(static key => key.Value, StringComparer.OrdinalIgnoreCase)],
                [.. Enumerable.Range(0, group.First().Transitions.Count).Select(index =>
                {
                    var sample = group.Select(flow => flow.Transitions[index].TimeInFromStatus).OrderBy(static value => value).ToList();
                    var middle = sample.Count / 2;
                    var median = sample.Count % 2 == 0
                        ? TimeSpan.FromTicks((sample[middle - 1].Ticks + sample[middle].Ticks) / 2)
                        : sample[middle];
                    var transition = group.First().Transitions[index];
                    return new FlowStageSummary(transition.From, transition.To, median);
                })]))
            .OrderByDescending(static group => group.Issues.Count)
            .ThenBy(static group => group.Path, StringComparer.OrdinalIgnoreCase)];
    }

    private static List<CountTable> ResolveCountTables(
        IReadOnlyList<JiraIssue> issues,
        IReadOnlyList<IssueFieldName>? configuredCountFields,
        IReadOnlyDictionary<string, string>? configuredCountFieldAliases)
    {
        var countFieldKeys = ResolveCountFieldKeys(configuredCountFields);
        var tables = new List<CountTable>();

        foreach (var countFieldKey in countFieldKeys)
        {
            var key = countFieldKey.Value;
            var issueFieldKey = new IssueKey(key);
            var displayName = TryGetFieldAlias(key, configuredCountFieldAliases, out var alias)
                ? alias
                : OutputColumnHeader.FromFieldKey(key).Value;
            tables.Add(new CountTable(
                $"By {displayName}",
                GroupByCount(issues, issueFieldKey)));
        }

        return tables;
    }

    private static List<IssueFieldName> ResolveCountFieldKeys(IReadOnlyList<IssueFieldName>? configuredCountFields)
        => ResolveConfiguredFieldKeys(configuredCountFields, _defaultCountOrder);

    private static IReadOnlyList<CountRow> GroupByCount(
        IReadOnlyList<JiraIssue> issues,
        IssueKey issueFieldKey)
    {
        return [.. issues
            .SelectMany(issue => ExpandCountValues(issue, issueFieldKey))
            .GroupBy(static value => value, StringComparer.OrdinalIgnoreCase)
            .Select(static group => new CountRow(group.Key, group.Count()))
            .OrderByDescending(static group => group.Count)
            .ThenBy(static group => group.Name, StringComparer.OrdinalIgnoreCase)];
    }

    private static List<string> ExpandCountValues(JiraIssue issue, IssueKey issueFieldKey)
    {
        var multiValues = issue.GetFieldValues(issueFieldKey);
        if (multiValues.Count > 0)
        {
            var expandedMultiValues = multiValues
                .Select(static value => value.Value)
                .Where(static value => !string.IsNullOrWhiteSpace(value) && value != "-")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            return expandedMultiValues.Count > 0 ? expandedMultiValues : ["Unknown"];
        }

        var value = issue.GetFieldValue(issueFieldKey);
        if (value == FieldValue.Missing || string.IsNullOrWhiteSpace(value.Value) || value.Value == "-")
        {
            return ["Unknown"];
        }

        return [value.Value];
    }

    private static List<IssueFieldName> ResolveConfiguredFieldKeys(
        IReadOnlyList<IssueFieldName>? configuredFields,
        IReadOnlyList<IssueFieldName> defaultFields)
    {
        ArgumentNullException.ThrowIfNull(defaultFields);

        var sourceFields = configuredFields is { Count: > 0 }
            ? configuredFields
            : defaultFields;
        var resolvedFields = new List<IssueFieldName>();
        var seenFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawField in sourceFields)
        {
            if (string.IsNullOrWhiteSpace(rawField.Value))
            {
                continue;
            }

            var normalizedField = rawField.Value.Trim();
            if (seenFields.Add(normalizedField))
            {
                resolvedFields.Add(new IssueFieldName(normalizedField));
            }
        }

        if (resolvedFields.Count > 0)
        {
            return resolvedFields;
        }

        return [.. defaultFields];
    }

    private static bool TryGetFieldAlias(
        string fieldKey,
        IReadOnlyDictionary<string, string>? configuredFieldAliases,
        out string alias)
    {
        alias = string.Empty;
        if (configuredFieldAliases is null || configuredFieldAliases.Count == 0)
        {
            return false;
        }

        return configuredFieldAliases.TryGetValue(fieldKey, out alias!) &&
            !string.IsNullOrWhiteSpace(alias);
    }

    private static readonly IReadOnlyList<IssueFieldName> _defaultOutputOrder =
    [
        new IssueFieldName("key"),
        new IssueFieldName("issuetype"),
        new IssueFieldName("status"),
        new IssueFieldName("assignee"),
        new IssueFieldName("created"),
        new IssueFieldName("updated"),
        new IssueFieldName("summary")
    ];

    private static readonly IReadOnlyList<IssueFieldName> _defaultCountOrder =
    [
        new IssueFieldName("status"),
        new IssueFieldName("issuetype"),
        new IssueFieldName("assignee")
    ];
}
