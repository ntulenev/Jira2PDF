using System.Text.Json.Serialization;

namespace JiraReport.Transport.Models;

internal sealed class JiraIssueFieldsResponse
{
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("status")]
    public JiraNamedEntityResponse? Status { get; set; }

    [JsonPropertyName("issuetype")]
    public JiraNamedEntityResponse? IssueType { get; set; }

    [JsonPropertyName("assignee")]
    public JiraAssigneeResponse? Assignee { get; set; }

    [JsonPropertyName("created")]
    public string? Created { get; set; }

    [JsonPropertyName("updated")]
    public string? Updated { get; set; }
}
