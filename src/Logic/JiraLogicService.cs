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
    public IReadOnlyList<OutputColumn> ResolveOutputColumns(IReadOnlyList<IssueFieldName>? configuredFields)
    {
        var fieldKeys = ResolveConfiguredFieldKeys(configuredFields, _defaultOutputOrder);
        var columns = new List<OutputColumn>(fieldKeys.Count);

        foreach (var fieldKey in fieldKeys)
        {
            var key = fieldKey.Value;
            var issueFieldKey = new IssueKey(key);
            columns.Add(new OutputColumn(
                issueFieldKey,
                OutputColumnHeader.FromFieldKey(key),
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
        IReadOnlyList<IssueFieldName>? configuredCountFields)
    {
        ArgumentNullException.ThrowIfNull(issues);

        var countTables = ResolveCountTables(issues, configuredCountFields);
        return new JiraJqlReport(reportTitle, configName, jql, DateTimeOffset.Now, issues, countTables);
    }

    private static List<CountTable> ResolveCountTables(
        IReadOnlyList<JiraIssue> issues,
        IReadOnlyList<IssueFieldName>? configuredCountFields)
    {
        var countFieldKeys = ResolveCountFieldKeys(configuredCountFields);
        var tables = new List<CountTable>();

        foreach (var countFieldKey in countFieldKeys)
        {
            var issueFieldKey = new IssueKey(countFieldKey.Value);
            tables.Add(new CountTable(
                $"By {OutputColumnHeader.FromFieldKey(countFieldKey.Value).Value}",
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
