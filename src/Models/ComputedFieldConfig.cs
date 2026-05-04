namespace JiraReport.Models;

/// <summary>
/// Defines a computed field that is derived from Jira data rather than returned directly by search.
/// </summary>
internal sealed record ComputedFieldConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ComputedFieldConfig"/> record.
    /// </summary>
    public ComputedFieldConfig(
        string type,
        string linkType,
        string mode,
        string metric,
        IReadOnlyList<string> doneStatusCategories,
        string childJqlTemplate,
        string format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(linkType);
        ArgumentException.ThrowIfNullOrWhiteSpace(mode);
        ArgumentException.ThrowIfNullOrWhiteSpace(metric);
        ArgumentNullException.ThrowIfNull(doneStatusCategories);
        ArgumentException.ThrowIfNullOrWhiteSpace(childJqlTemplate);
        ArgumentException.ThrowIfNullOrWhiteSpace(format);

        Type = type.Trim();
        LinkType = linkType.Trim();
        Mode = mode.Trim();
        Metric = metric.Trim();
        DoneStatusCategories = [.. doneStatusCategories
            .Where(static category => !string.IsNullOrWhiteSpace(category))
            .Select(static category => category.Trim())];
        ChildJqlTemplate = childJqlTemplate.Trim();
        Format = format.Trim();
    }

    /// <summary>
    /// Gets computed field type.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets issue link type used to find linked delivery work items.
    /// </summary>
    public string LinkType { get; }

    /// <summary>
    /// Gets calculation mode.
    /// </summary>
    public string Mode { get; }

    /// <summary>
    /// Gets calculation metric.
    /// </summary>
    public string Metric { get; }

    /// <summary>
    /// Gets status categories counted as done.
    /// </summary>
    public IReadOnlyList<string> DoneStatusCategories { get; }

    /// <summary>
    /// Gets JQL template used to load child work items for linked work items.
    /// </summary>
    public string ChildJqlTemplate { get; }

    /// <summary>
    /// Gets display format.
    /// </summary>
    public string Format { get; }
}
