using System.Text.Json.Serialization;

namespace JiraReport.Transport.Models;

/// <summary>
/// DTO for Jira issue item in search response.
/// </summary>
internal sealed class JiraIssueResponse
{
    /// <summary>
    /// Gets or sets issue key.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets issue fields payload.
    /// </summary>
    [JsonPropertyName("fields")]
    public JiraIssueFieldsResponse? Fields { get; set; }
}
