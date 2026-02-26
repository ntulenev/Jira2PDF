namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents validated PDF file path value.
/// </summary>
internal readonly record struct PdfFilePath
{
    /// <summary>
    /// Initializes a new <see cref="PdfFilePath"/> instance.
    /// </summary>
    /// <param name="value">Raw PDF file path value.</param>
    public PdfFilePath(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Gets normalized PDF file path text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Builds default PDF file path from report title and generation timestamp.
    /// </summary>
    /// <param name="reportTitle">Report title.</param>
    /// <param name="generatedAt">Generation timestamp.</param>
    /// <returns>Default PDF file path.</returns>
    public static PdfFilePath FromReportTitle(PdfReportName reportTitle, DateTimeOffset generatedAt)
    {
        var sanitizedTitle = SanitizeFileName(reportTitle.Value);
        if (string.IsNullOrWhiteSpace(sanitizedTitle))
        {
            sanitizedTitle = "jql-report";
        }

        var timestampedFileName = $"{sanitizedTitle}_{generatedAt:yyyyMMdd_HHmmss}.pdf";
        return new PdfFilePath(Path.GetFullPath(timestampedFileName));
    }

    /// <summary>
    /// Returns PDF file path text representation.
    /// </summary>
    /// <returns>PDF file path text.</returns>
    public override string ToString() => Value;

    private static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string([.. value
            .Trim()
            .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
        ]);

        return string.IsNullOrWhiteSpace(sanitized)
            ? string.Empty
            : sanitized.Replace(' ', '_');
    }
}
