namespace JiraReport.Models.Configuration;

/// <summary>
/// Represents raw computed field config from configuration.
/// </summary>
internal sealed class ComputedFieldOptions
{
    /// <summary>
    /// Gets computed field type.
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Gets issue link type used to find linked delivery work items.
    /// </summary>
    public string? LinkType { get; init; }

    /// <summary>
    /// Gets calculation mode.
    /// </summary>
    public string? Mode { get; init; }

    /// <summary>
    /// Gets calculation metric.
    /// </summary>
    public string? Metric { get; init; }

    /// <summary>
    /// Gets status categories counted as done.
    /// </summary>
    public IReadOnlyList<string>? DoneStatusCategories { get; init; }

    /// <summary>
    /// Gets JQL template used to load child work items for linked work items.
    /// </summary>
    public string? ChildJqlTemplate { get; init; }

    /// <summary>
    /// Gets display format.
    /// </summary>
    public string? Format { get; init; }
}
