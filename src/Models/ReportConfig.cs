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
    /// <param name="outputFieldsAliases">Optional display aliases for requested output fields.</param>
    /// <param name="countFieldsAliases">Optional display aliases for requested grouped count fields.</param>
    public ReportConfig(
        ReportName name,
        JqlQuery jql,
        IReadOnlyList<IssueFieldName> outputFields,
        IReadOnlyList<IssueFieldName> countFields,
        PdfReportName pdfReportName,
        IReadOnlyDictionary<string, string>? outputFieldsAliases = null,
        IReadOnlyDictionary<string, string>? countFieldsAliases = null)
    {
        ArgumentNullException.ThrowIfNull(outputFields);
        ArgumentNullException.ThrowIfNull(countFields);

        Name = name;
        Jql = jql;
        OutputFields = [.. outputFields];
        CountFields = [.. countFields];
        PdfReportName = pdfReportName;
        OutputFieldsAliases = NormalizeAliases(outputFieldsAliases);
        CountFieldsAliases = NormalizeAliases(countFieldsAliases);
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
    /// Gets optional display aliases for requested output fields.
    /// </summary>
    public IReadOnlyDictionary<string, string> OutputFieldsAliases { get; }

    /// <summary>
    /// Gets requested grouped count fields.
    /// </summary>
    public IReadOnlyList<IssueFieldName> CountFields { get; }

    /// <summary>
    /// Gets optional display aliases for requested grouped count fields.
    /// </summary>
    public IReadOnlyDictionary<string, string> CountFieldsAliases { get; }

    /// <summary>
    /// Gets required PDF report title/file base name.
    /// </summary>
    public PdfReportName PdfReportName { get; }

    private static IReadOnlyDictionary<string, string> NormalizeAliases(
        IReadOnlyDictionary<string, string>? aliases)
    {
        if (aliases is null || aliases.Count == 0)
        {
            return _emptyAliases;
        }

        var normalizedAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (field, alias) in aliases)
        {
            if (string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(alias))
            {
                continue;
            }

            normalizedAliases[field.Trim()] = alias.Trim();
        }

        return normalizedAliases.Count > 0 ? normalizedAliases : _emptyAliases;
    }

    private static readonly IReadOnlyDictionary<string, string> _emptyAliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
