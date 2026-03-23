using System.ComponentModel.DataAnnotations;

namespace JiraReport.Models.Configuration;

/// <summary>
/// Represents raw console UI configuration values from the root <c>UI</c> section.
/// </summary>
internal sealed class UiOptions
{
    /// <summary>
    /// Gets the number of report items visible in the selection prompt.
    /// </summary>
    [Range(1, 100)]
    public int? ReportSelectionPageSize { get; init; }
}
