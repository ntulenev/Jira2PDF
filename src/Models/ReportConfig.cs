using JiraReport.Models.ValueObjects;

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
        ReportName name,
        JqlQuery jql,
        IReadOnlyList<IssueFieldName> outputFields,
        IReadOnlyList<IssueFieldName> countFields,
        PdfReportName pdfReportName)
    {
        ArgumentNullException.ThrowIfNull(outputFields);
        ArgumentNullException.ThrowIfNull(countFields);

        Name = name;
        Jql = jql;
        OutputFields = [.. outputFields];
        CountFields = [.. countFields];
        PdfReportName = pdfReportName;
    }

    /// <summary>
    /// Gets configuration name.
    /// </summary>
    public ReportName Name { get; }

    /// <summary>
    /// Gets JQL query.
    /// </summary>
    public JqlQuery Jql { get; }

    /// <summary>
    /// Gets requested output fields.
    /// </summary>
    public IReadOnlyList<IssueFieldName> OutputFields { get; }

    /// <summary>
    /// Gets requested grouped count fields.
    /// </summary>
    public IReadOnlyList<IssueFieldName> CountFields { get; }

    /// <summary>
    /// Gets required PDF report title/file base name.
    /// </summary>
    public PdfReportName PdfReportName { get; }
}
