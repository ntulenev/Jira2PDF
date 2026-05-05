namespace JiraReport.Models.Configuration;

/// <summary>
/// Represents raw field value converter config from configuration.
/// </summary>
internal sealed class FieldValueConverterOptions
{
    /// <summary>
    /// Gets converter type.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Gets JSON path to extract from the raw Jira field value.
    /// </summary>
    public string? JsonPath { get; init; }
}
