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
    /// Returns PDF file path text representation.
    /// </summary>
    /// <returns>PDF file path text.</returns>
    public override string ToString() => Value;
}
