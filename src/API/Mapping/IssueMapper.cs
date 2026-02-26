using System.Globalization;
using System.Text.Json;

using JiraReport.Abstractions;
using JiraReport.Models;
using JiraReport.Models.ValueObjects;
using JiraReport.Transport.Models;

namespace JiraReport.API.Mapping;

/// <summary>
/// Default mapper from Jira transport search results to domain issues.
/// </summary>
internal sealed class IssueMapper : IIssueMapper
{
    /// <inheritdoc />
    public IReadOnlyList<JiraIssue> MapIssues(
        JiraSearchResponse page,
        IReadOnlyDictionary<string, IReadOnlyList<string>> aliasesByApiField)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(aliasesByApiField);

        if (page.Issues.Count == 0)
        {
            return [];
        }

        return [.. page.Issues
            .Where(static issue => !string.IsNullOrWhiteSpace(issue.Key))
            .Select(issue =>
            {
                var fields = new Dictionary<IssueKey, FieldValue>();
                if (issue.Fields?.Values is { Count: > 0 })
                {
                    foreach (var (fieldKey, rawValue) in issue.Fields.Values)
                    {
                        var normalizedFieldKey = NormalizeFieldKey(fieldKey);
                        var fieldValue = NormalizeFieldValue(normalizedFieldKey, rawValue);
                        fields[new IssueKey(normalizedFieldKey)] = new FieldValue(fieldValue);

                        if (aliasesByApiField.TryGetValue(normalizedFieldKey, out var aliases))
                        {
                            foreach (var alias in aliases)
                            {
                                fields[new IssueKey(alias)] = new FieldValue(fieldValue);
                            }
                        }
                    }
                }

                return new JiraIssue(new IssueKey(issue.Key!.Trim()), fields);
            })];
    }

    private static string NormalizeFieldValue(string fieldKey, JsonElement rawValue)
    {
        var value = ExtractJsonValue(rawValue);
        if (string.IsNullOrWhiteSpace(value) || value == "-")
        {
            return "-";
        }

        var trimmed = value.Trim();
        if (IsDateField(fieldKey) &&
            DateTimeOffset.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        return trimmed;
    }

    private static string ExtractJsonValue(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
                return value.GetString() ?? "-";
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                return value.ToString();
            case JsonValueKind.Array:
            {
                var items = value
                    .EnumerateArray()
                    .Select(ExtractJsonValue)
                    .Where(static item => !string.IsNullOrWhiteSpace(item) && item != "-")
                    .ToList();
                return items.Count == 0 ? "-" : string.Join(", ", items);
            }

            case JsonValueKind.Object:
            {
                if (TryGetObjectDisplayValue(value, "displayName", out var displayName))
                {
                    return displayName;
                }

                if (TryGetObjectDisplayValue(value, "name", out var name))
                {
                    return name;
                }

                if (TryGetObjectDisplayValue(value, "value", out var optionValue))
                {
                    return optionValue;
                }

                if (TryGetObjectDisplayValue(value, "key", out var key))
                {
                    return key;
                }

                return "-";
            }

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return "-";
            default:
                return "-";
        }
    }

    private static bool TryGetObjectDisplayValue(JsonElement value, string propertyName, out string result)
    {
        result = string.Empty;

        if (!value.TryGetProperty(propertyName, out var propertyValue))
        {
            return false;
        }

        var extracted = ExtractJsonValue(propertyValue);
        if (string.IsNullOrWhiteSpace(extracted) || extracted == "-")
        {
            return false;
        }

        result = extracted.Trim();
        return true;
    }

    private static bool IsDateField(string fieldKey) =>
        string.Equals(fieldKey, "created", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(fieldKey, "updated", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeFieldKey(string fieldKey) => fieldKey.Trim();
}
