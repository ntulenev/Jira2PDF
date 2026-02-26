using System.Text.Json.Serialization;

namespace JiraReport.Transport.Models;

/// <summary>
/// DTO for selected Jira issue fields.
/// </summary>
internal sealed class JiraIssueFieldsResponse
{
    /// <summary>
    /// Gets or sets issue summary.
    /// </summary>
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// Gets or sets status descriptor.
    /// </summary>
    [JsonPropertyName("status")]
    public JiraNamedEntityResponse? Status { get; set; }

    /// <summary>
    /// Gets or sets issue type descriptor.
    /// </summary>
    [JsonPropertyName("issuetype")]
    public JiraNamedEntityResponse? IssueType { get; set; }

    /// <summary>
    /// Gets or sets assignee descriptor.
    /// </summary>
    [JsonPropertyName("assignee")]
    public JiraAssigneeResponse? Assignee { get; set; }

    /// <summary>
    /// Gets or sets raw created timestamp string.
    /// </summary>
    [JsonPropertyName("created")]
    public string? Created { get; set; }

    /// <summary>
    /// Gets or sets raw updated timestamp string.
    /// </summary>
    [JsonPropertyName("updated")]
    public string? Updated { get; set; }
}
