using System.Text.Json.Serialization;

namespace JiraReport.Transport.Models;

/// <summary>
/// DTO for Jira assignee object.
/// </summary>
internal sealed class JiraAssigneeResponse
{
    /// <summary>
    /// Gets or sets assignee display name.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}
