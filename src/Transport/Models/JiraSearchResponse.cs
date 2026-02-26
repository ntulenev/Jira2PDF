using System.Text.Json.Serialization;

namespace JiraReport.Transport.Models;

internal sealed class JiraSearchResponse
{
    [JsonPropertyName("issues")]
    public List<JiraIssueResponse> Issues { get; set; } = [];

    [JsonPropertyName("isLast")]
    public bool IsLast { get; set; }

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}
