using System.Text.Json.Serialization;

namespace JiraReport.Transport.Models;

/// <summary>
/// DTO for Jira field object that exposes <c>name</c>.
/// </summary>
internal sealed class JiraNamedEntityResponse
{
    /// <summary>
    /// Gets or sets entity name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
