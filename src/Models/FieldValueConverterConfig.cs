namespace JiraReport.Models;

/// <summary>
/// Represents validated field value converter config.
/// </summary>
internal sealed record FieldValueConverterConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FieldValueConverterConfig"/> record.
    /// </summary>
    /// <param name="type">Converter type.</param>
    /// <param name="jsonPath">JSON path to extract from the raw Jira field value.</param>
    public FieldValueConverterConfig(string type, string jsonPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);

        Type = type.Trim();
        JsonPath = jsonPath.Trim();
    }

    /// <summary>
    /// Gets converter type.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets JSON path to extract from the raw Jira field value.
    /// </summary>
    public string JsonPath { get; }
}
