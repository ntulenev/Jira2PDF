using System.Text.Json.Serialization;

namespace JiraReport.Transport.Models;

internal sealed class JiraAssigneeResponse
{
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}
