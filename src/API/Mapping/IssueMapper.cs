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
                var multiValueFields = new Dictionary<IssueKey, IReadOnlyList<FieldValue>>();
                if (issue.Fields?.Values is { Count: > 0 })
                {
                    foreach (var (fieldKey, rawValue) in issue.Fields.Values)
                    {
                        var normalizedFieldKey = NormalizeFieldKey(fieldKey);
                        var normalizedValues = NormalizeFieldValues(normalizedFieldKey, rawValue);
                        var flattenedValue = normalizedValues.Count == 0
                            ? "-"
                            : string.Join(", ", normalizedValues);
                        var issueFieldKey = new IssueKey(normalizedFieldKey);
                        fields[issueFieldKey] = new FieldValue(flattenedValue);

                        List<FieldValue>? fieldItems = null;
                        if (rawValue.ValueKind == JsonValueKind.Array && normalizedValues.Count > 0)
                        {
                            fieldItems = [.. normalizedValues.Select(static item => new FieldValue(item))];
                            multiValueFields[issueFieldKey] = fieldItems;
                        }

                        if (aliasesByApiField.TryGetValue(normalizedFieldKey, out var aliases))
                        {
                            foreach (var alias in aliases)
                            {
                                var aliasKey = new IssueKey(alias);
                                fields[aliasKey] = new FieldValue(flattenedValue);
                                if (fieldItems is not null)
                                {
                                    multiValueFields[aliasKey] = fieldItems;
                                }
                            }
                        }
                    }
                }

                return new JiraIssue(new IssueKey(issue.Key!.Trim()), fields, multiValueFields);
            })];
    }

    private static List<string> NormalizeFieldValues(string fieldKey, JsonElement rawValue)
    {
        var values = ExtractJsonValues(rawValue);
        if (values.Count == 0)
        {
            return [];
        }

        var normalizedValues = new List<string>(values.Count);
        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "-")
            {
                continue;
            }

            var normalizedValue = value.Trim();
            if (IsDateField(fieldKey) &&
                DateTimeOffset.TryParse(
                    normalizedValue,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                normalizedValue = parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }

            normalizedValues.Add(normalizedValue);
        }

        return [.. normalizedValues.Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    private static List<string> ExtractJsonValues(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
            {
                var stringValue = value.GetString();
                return string.IsNullOrWhiteSpace(stringValue) ? [] : [stringValue];
            }

            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                return [value.ToString()];

            case JsonValueKind.Array:
                return [.. value.EnumerateArray().SelectMany(ExtractJsonValues)];

            case JsonValueKind.Object:
                return ExtractObjectDisplayValues(value);

            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return [];

            default:
                return [];
        }
    }

    private static List<string> ExtractObjectDisplayValues(JsonElement value)
    {
        foreach (var propertyName in _objectDisplayPropertyOrder)
        {
            if (TryGetObjectDisplayValues(value, propertyName, out var values))
            {
                return values;
            }
        }

        return [];
    }

    private static bool TryGetObjectDisplayValues(
        JsonElement value,
        string propertyName,
        out List<string> values)
    {
        values = [];
        if (!value.TryGetProperty(propertyName, out var propertyValue))
        {
            return false;
        }

        values = [.. ExtractJsonValues(propertyValue)
            .Where(static item => !string.IsNullOrWhiteSpace(item) && item != "-")
            .Select(static item => item.Trim())];
        return values.Count > 0;
    }

    private static bool IsDateField(string fieldKey) =>
        string.Equals(fieldKey, "created", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(fieldKey, "updated", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeFieldKey(string fieldKey) => fieldKey.Trim();
    private static readonly IReadOnlyList<string> _objectDisplayPropertyOrder =
        ["displayName", "name", "value", "key"];
}
