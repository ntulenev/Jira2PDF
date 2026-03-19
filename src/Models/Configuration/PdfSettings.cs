namespace JiraReport.Models.Configuration;

/// <summary>
/// Represents validated PDF behavior settings.
/// </summary>
/// <param name="OpenAfterGeneration">Whether generated PDF should be opened after the workflow finishes.</param>
internal sealed record PdfSettings(bool OpenAfterGeneration);
