using System.Text.Json.Serialization;

namespace JiraReport.Transport.Models;

internal sealed class JiraIssueResponse
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("fields")]
    public JiraIssueFieldsResponse? Fields { get; set; }
}
