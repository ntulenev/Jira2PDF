using JiraReport.Abstractions;
using JiraReport.Models;

namespace JiraReport.Logic;

/// <summary>
/// Default domain logic implementation for report preparation.
/// </summary>
internal sealed class JiraLogicService : IJiraLogicService
{
    /// <inheritdoc />
    public string ResolveReportTitle(ReportConfig? selectedReportConfig)
    {
        if (selectedReportConfig is null)
        {
            return "Jira JQL Report";
        }

        return selectedReportConfig.PdfReportName.Trim();
    }

    /// <inheritdoc />
    public IReadOnlyList<OutputColumn> ResolveOutputColumns(IReadOnlyList<string>? configuredFields)
    {
        var fieldKeys = ResolveConfiguredFieldKeys(configuredFields, _defaultOutputOrder);
        var columns = new List<OutputColumn>(fieldKeys.Count);

        foreach (var fieldKey in fieldKeys)
        {
            var key = fieldKey;
            columns.Add(new OutputColumn(
                key,
                BuildFieldHeader(key),
                ResolveConsoleWidth(key),
                issue => issue.GetFieldValue(key)));
        }

        return columns;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> ResolveRequestedIssueFields(
        IReadOnlyList<string>? configuredOutputFields,
        IReadOnlyList<string>? configuredCountFields)
    {
        var outputFields = ResolveOutputColumns(configuredOutputFields).Select(static column => column.Key);
        var countFields = ResolveCountFieldKeys(configuredCountFields);

        var requestedFields = new List<string>();
        var seenFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in outputFields)
        {
            if (seenFields.Add(field))
            {
                requestedFields.Add(field);
            }
        }

        foreach (var field in countFields)
        {
            if (seenFields.Add(field))
            {
                requestedFields.Add(field);
            }
        }

        return requestedFields;
    }

    /// <inheritdoc />
    public string BuildDefaultPdfPath(string reportTitle, DateTimeOffset generatedAt)
    {
        var sanitizedTitle = SanitizeFileName(reportTitle);
        if (string.IsNullOrWhiteSpace(sanitizedTitle))
        {
            sanitizedTitle = "jql-report";
        }

        var timestampedFileName = $"{sanitizedTitle}_{generatedAt:yyyyMMdd_HHmmss}.pdf";
        return Path.GetFullPath(timestampedFileName);
    }

    /// <inheritdoc />
    public JiraJqlReport BuildReport(
        string reportTitle,
        string? configName,
        string jql,
        IReadOnlyList<JiraIssue> issues,
        IReadOnlyList<string>? configuredCountFields)
    {
        ArgumentNullException.ThrowIfNull(issues);

        var countTables = ResolveCountTables(issues, configuredCountFields);
        return JiraJqlReport.Create(reportTitle, configName, jql, issues, countTables, DateTimeOffset.Now);
    }

    private static List<CountTable> ResolveCountTables(
        IReadOnlyList<JiraIssue> issues,
        IReadOnlyList<string>? configuredCountFields)
    {
        var countFieldKeys = ResolveCountFieldKeys(configuredCountFields);
        var tables = new List<CountTable>();

        foreach (var countFieldKey in countFieldKeys)
        {
            tables.Add(new CountTable(
                $"By {BuildFieldHeader(countFieldKey)}",
                GroupByCount(issues, issue => issue.GetFieldValue(countFieldKey))));
        }

        return tables;
    }

    private static List<string> ResolveCountFieldKeys(IReadOnlyList<string>? configuredCountFields)
    {
        return ResolveConfiguredFieldKeys(configuredCountFields, _defaultCountOrder);
    }

    private static IReadOnlyList<CountRow> GroupByCount(
        IReadOnlyList<JiraIssue> issues,
        Func<JiraIssue, string> selector)
    {
        return [.. issues
            .GroupBy(
                issue =>
                {
                    var value = selector(issue);
                    return string.IsNullOrWhiteSpace(value) || value == "-" ? "Unknown" : value.Trim();
                },
                StringComparer.OrdinalIgnoreCase)
            .Select(static group => new CountRow(group.Key, group.Count()))
            .OrderByDescending(static group => group.Count)
            .ThenBy(static group => group.Name, StringComparer.OrdinalIgnoreCase)];
    }

    private static List<string> ResolveConfiguredFieldKeys(
        IReadOnlyList<string>? configuredFields,
        IReadOnlyList<string> defaultFields)
    {
        ArgumentNullException.ThrowIfNull(defaultFields);

        var sourceFields = configuredFields is { Count: > 0 } ? configuredFields : defaultFields;
        var resolvedFields = new List<string>();
        var seenFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawField in sourceFields)
        {
            if (string.IsNullOrWhiteSpace(rawField))
            {
                continue;
            }

            var normalizedField = rawField.Trim();
            if (seenFields.Add(normalizedField))
            {
                resolvedFields.Add(normalizedField);
            }
        }

        if (resolvedFields.Count > 0)
        {
            return resolvedFields;
        }

        return [.. defaultFields];
    }

    private static string BuildFieldHeader(string fieldKey)
    {
        if (string.IsNullOrWhiteSpace(fieldKey))
        {
            return "Field";
        }

        var words = fieldKey
            .Trim()
            .Replace('_', ' ')
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length == 0)
        {
            return "Field";
        }

        var headerWords = new List<string>(words.Length);
        foreach (var word in words)
        {
            var characters = word.ToCharArray();
            characters[0] = char.ToUpperInvariant(characters[0]);
            headerWords.Add(new string(characters));
        }

        return string.Join(' ', headerWords);
    }

    private static int ResolveConsoleWidth(string fieldKey) =>
        string.Equals(fieldKey, "summary", StringComparison.OrdinalIgnoreCase) ? 52 : 20;

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string([.. value
            .Trim()
            .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
        ]);

        return string.IsNullOrWhiteSpace(sanitized)
            ? string.Empty
            : sanitized.Replace(' ', '_');
    }

    private static readonly IReadOnlyList<string> _defaultOutputOrder =
        ["key", "issuetype", "status", "assignee", "created", "updated", "summary"];

    private static readonly IReadOnlyList<string> _defaultCountOrder =
        ["status", "issuetype", "assignee"];
}
