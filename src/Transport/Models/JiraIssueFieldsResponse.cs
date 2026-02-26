using System.Text.Json.Serialization;
using System.Text.Json;

namespace JiraReport.Transport.Models;

/// <summary>
/// DTO for Jira issue fields payload.
/// </summary>
internal sealed class JiraIssueFieldsResponse
{
    /// <summary>
    /// Gets or sets raw field values.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> Values { get; set; } = [];
}
