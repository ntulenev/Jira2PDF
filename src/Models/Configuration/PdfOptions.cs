namespace JiraReport.Models.Configuration;

/// <summary>
/// Represents raw PDF behavior configuration values from the root <c>PDF</c> section.
/// </summary>
internal sealed class PdfOptions
{
    /// <summary>
    /// Gets a value indicating whether generated PDF should be opened after the workflow finishes.
    /// </summary>
    public bool OpenAfterGeneration { get; init; }
}
