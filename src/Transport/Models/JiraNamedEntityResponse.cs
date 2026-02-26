using System.Text.Json.Serialization;

namespace JiraReport.Transport.Models;

internal sealed class JiraNamedEntityResponse
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
