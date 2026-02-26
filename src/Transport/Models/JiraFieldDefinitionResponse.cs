using System.Text.Json.Serialization;

namespace JiraReport.Transport.Models;

/// <summary>
/// DTO for Jira field definition from fields catalog endpoints.
/// </summary>
internal sealed class JiraFieldDefinitionResponse
{
    /// <summary>
    /// Gets or sets field id.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets field key.
    /// </summary>
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    /// <summary>
    /// Gets or sets display field name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets JQL clause aliases for the field.
    /// </summary>
    [JsonPropertyName("clauseNames")]
    public List<string> ClauseNames { get; set; } = [];
}
