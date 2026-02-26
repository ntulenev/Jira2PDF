namespace JiraReport.Models;

/// <summary>
/// Represents one named report configuration entry.
/// </summary>
internal sealed record ReportConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportConfig"/> record.
    /// </summary>
    /// <param name="name">Configuration name.</param>
    /// <param name="jql">JQL query.</param>
    /// <param name="outputFields">Requested output fields.</param>
    /// <param name="countFields">Requested grouped count fields.</param>
    /// <param name="pdfReportName">Required PDF report title/file base name.</param>
    public ReportConfig(
        string name,
        string jql,
        IReadOnlyList<string> outputFields,
        IReadOnlyList<string> countFields,
        string pdfReportName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(jql);
        ArgumentNullException.ThrowIfNull(outputFields);
        ArgumentNullException.ThrowIfNull(countFields);
        ArgumentException.ThrowIfNullOrWhiteSpace(pdfReportName);

        Name = name.Trim();
        Jql = jql.Trim();
        OutputFields = outputFields;
        CountFields = countFields;
        PdfReportName = pdfReportName.Trim();
    }

    /// <summary>
    /// Gets configuration name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets JQL query.
    /// </summary>
    public string Jql { get; }

    /// <summary>
    /// Gets requested output fields.
    /// </summary>
    public IReadOnlyList<string> OutputFields { get; }

    /// <summary>
    /// Gets requested grouped count fields.
    /// </summary>
    public IReadOnlyList<string> CountFields { get; }

    /// <summary>
    /// Gets required PDF report title/file base name.
    /// </summary>
    public string PdfReportName { get; }
}
