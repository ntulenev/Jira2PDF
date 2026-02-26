using System.Text.Json.Serialization;

namespace JiraReport.Transport.Models;

/// <summary>
/// DTO for Jira search page response.
/// </summary>
internal sealed class JiraSearchResponse
{
    /// <summary>
    /// Gets or sets page issue items.
    /// </summary>
    [JsonPropertyName("issues")]
    public List<JiraIssueResponse> Issues { get; set; } = [];

    /// <summary>
    /// Gets or sets flag indicating whether this is the last page.
    /// </summary>
    [JsonPropertyName("isLast")]
    public bool IsLast { get; set; }

    /// <summary>
    /// Gets or sets next page token for pagination.
    /// </summary>
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }

    /// <summary>
    /// Gets or sets total issue count.
    /// </summary>
    [JsonPropertyName("total")]
    public int Total { get; set; }
}
