using System.Text.Json;
using System.Text.Json.Serialization;

namespace JiraReport.Transport.Models;

internal sealed class JiraBulkChangelogRequest
{
    [JsonPropertyName("fieldIds")] public IReadOnlyList<string> FieldIds { get; init; } = ["status"];
    [JsonPropertyName("issueIdsOrKeys")] public required IReadOnlyList<string> IssueIdsOrKeys { get; init; }
    [JsonPropertyName("maxResults")] public int MaxResults { get; init; } = 1000;
    [JsonPropertyName("nextPageToken")] public string? NextPageToken { get; init; }
}

internal sealed class JiraBulkChangelogResponse
{
    [JsonPropertyName("issueChangeLogs")] public IReadOnlyList<JiraBulkIssueChangelog> IssueChangeLogs { get; init; } = [];
    [JsonPropertyName("nextPageToken")] public string? NextPageToken { get; init; }
}

internal sealed class JiraBulkIssueChangelog
{
    [JsonPropertyName("issueId")] public string? IssueId { get; init; }
    [JsonPropertyName("changeHistories")] public IReadOnlyList<JiraBulkHistory> ChangeHistories { get; init; } = [];
}

internal sealed class JiraBulkHistory
{
    [JsonPropertyName("created")] public JsonElement Created { get; init; }
    [JsonPropertyName("items")] public IReadOnlyList<JiraHistoryItem> Items { get; init; } = [];
}

internal sealed class JiraHistoryItem
{
    [JsonPropertyName("field")] public string? Field { get; init; }
    [JsonPropertyName("fromString")] public string? From { get; init; }
    [JsonPropertyName("toString")] public string? To { get; init; }
}
