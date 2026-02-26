namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents validated PDF report name value.
/// </summary>
internal readonly record struct PdfReportName
{
    /// <summary>
    /// Initializes a new <see cref="PdfReportName"/> instance.
    /// </summary>
    /// <param name="value">Raw report name value.</param>
    public PdfReportName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Gets normalized report name text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Returns report name text representation.
    /// </summary>
    /// <returns>Report name text.</returns>
    public override string ToString() => Value;
}
