using System.Globalization;

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

        if (!string.IsNullOrWhiteSpace(selectedReportConfig.PdfReportName))
        {
            return selectedReportConfig.PdfReportName.Trim();
        }

        return selectedReportConfig.Name.Trim();
    }

    /// <inheritdoc />
    public IReadOnlyList<OutputColumn> ResolveOutputColumns(IReadOnlyList<string>? configuredFields)
    {
        var fields = configuredFields is { Count: > 0 } ? configuredFields : _defaultOutputOrder;
        var resolvedColumns = new List<OutputColumn>();
        var seenColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawField in fields)
        {
            if (string.IsNullOrWhiteSpace(rawField))
            {
                continue;
            }

            var normalizedField = rawField.Trim();
            if (!_columns.TryGetValue(normalizedField, out var column))
            {
                throw new InvalidOperationException(
                    $"Unsupported output field '{normalizedField}'. Supported values: {string.Join(", ", _columns.Keys)}.");
            }

            if (seenColumns.Add(column.Key))
            {
                resolvedColumns.Add(column);
            }
        }

        if (resolvedColumns.Count > 0)
        {
            return resolvedColumns;
        }

        return [.. _defaultOutputOrder.Select(static field => _columns[field])];
    }

    /// <inheritdoc />
    public string BuildDefaultPdfPath(string configuredPath, string reportTitle, DateTimeOffset generatedAt)
    {
        var basePath = string.IsNullOrWhiteSpace(configuredPath)
            ? "jql-report.pdf"
            : configuredPath.Trim();

        if (!basePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            basePath += ".pdf";
        }

        var directory = Path.GetDirectoryName(basePath);
        var extension = Path.GetExtension(basePath);
        var fallbackName = Path.GetFileNameWithoutExtension(basePath);
        var selectedName = string.IsNullOrWhiteSpace(reportTitle)
            ? fallbackName
            : SanitizeFileName(reportTitle);
        if (string.IsNullOrWhiteSpace(selectedName))
        {
            selectedName = fallbackName;
        }

        var timestampedFileName = $"{selectedName}_{generatedAt:yyyyMMdd_HHmmss}{extension}";
        return string.IsNullOrWhiteSpace(directory)
            ? Path.GetFullPath(timestampedFileName)
            : Path.GetFullPath(Path.Combine(directory, timestampedFileName));
    }

    /// <inheritdoc />
    public JiraJqlReport BuildReport(
        string reportTitle,
        string? configName,
        string jql,
        IReadOnlyList<JiraIssue> issues) =>
        JiraJqlReport.Create(reportTitle, configName, jql, issues, DateTimeOffset.Now);

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

    private static readonly IReadOnlyDictionary<string, OutputColumn> _columns =
        new Dictionary<string, OutputColumn>(StringComparer.OrdinalIgnoreCase)
        {
            ["key"] = new OutputColumn("key", "Key", 12, static issue => issue.Key),
            ["summary"] = new OutputColumn("summary", "Summary", 52, static issue => issue.Summary),
            ["status"] = new OutputColumn("status", "Status", 18, static issue => issue.Status),
            ["issuetype"] = new OutputColumn("issuetype", "Type", 14, static issue => issue.IssueType),
            ["assignee"] = new OutputColumn("assignee", "Assignee", 24, static issue => issue.Assignee),
            ["created"] = new OutputColumn(
                "created",
                "Created",
                10,
                static issue => issue.Created.HasValue
                    ? issue.Created.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : "-"),
            ["updated"] = new OutputColumn(
                "updated",
                "Updated",
                10,
                static issue => issue.Updated.HasValue
                    ? issue.Updated.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    : "-")
        };

    private static readonly IReadOnlyList<string> _defaultOutputOrder =
        ["key", "issuetype", "status", "assignee", "created", "updated", "summary"];
}
