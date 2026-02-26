namespace JiraReport.Models.Configuration;

/// <summary>
/// Represents one raw report config entry from configuration.
/// </summary>
internal sealed class ReportConfigOptions
{
    /// <summary>
    /// Gets config name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets config JQL query.
    /// </summary>
    public string? Jql { get; init; }

    /// <summary>
    /// Gets requested output fields.
    /// </summary>
    public IReadOnlyList<string>? OutputFields { get; init; }

    /// <summary>
    /// Gets requested grouped count fields.
    /// </summary>
    public IReadOnlyList<string>? CountFields { get; init; }

    /// <summary>
    /// Gets required PDF report title/file base name.
    /// </summary>
    public string? PdfReportName { get; init; }
}
