namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents validated CSV file path value.
/// </summary>
internal readonly record struct CsvFilePath
{
    /// <summary>
    /// Initializes a new <see cref="CsvFilePath"/> instance.
    /// </summary>
    /// <param name="value">Raw CSV file path value.</param>
    public CsvFilePath(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Gets normalized CSV file path text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Builds CSV file path that pairs with the provided PDF output path.
    /// </summary>
    /// <param name="pdfPath">Resolved PDF file path.</param>
    /// <returns>CSV file path with <c>_raw</c> suffix.</returns>
    public static CsvFilePath FromPdfPath(PdfFilePath pdfPath)
    {
        var directoryPath = Path.GetDirectoryName(pdfPath.Value);
        var fileName = Path.GetFileNameWithoutExtension(pdfPath.Value);
        var csvFileName = $"{fileName}_raw.csv";
        var combinedPath = string.IsNullOrWhiteSpace(directoryPath)
            ? csvFileName
            : Path.Combine(directoryPath, csvFileName);

        return new CsvFilePath(Path.GetFullPath(combinedPath));
    }

    /// <summary>
    /// Returns CSV file path text representation.
    /// </summary>
    /// <returns>CSV file path text.</returns>
    public override string ToString() => Value;
}
