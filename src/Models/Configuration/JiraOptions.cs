using System.ComponentModel.DataAnnotations;

namespace JiraReport.Models.Configuration;

internal sealed class JiraOptions
{
    public Uri? BaseUrl { get; init; }

    public string? Email { get; init; }

    public string? ApiToken { get; init; }

    [Range(1, 100)]
    public int MaxResultsPerPage { get; init; } = 100;

    [Range(0, 10)]
    public int RetryCount { get; init; } = 3;

    public string? DefaultPdfPath { get; init; }

    public IReadOnlyList<ReportConfigOptions>? Reports { get; init; }
}

internal sealed class ReportConfigOptions
{
    public string? Name { get; init; }

    public string? Jql { get; init; }

    public IReadOnlyList<string>? OutputFields { get; init; }

    public string? PdfReportName { get; init; }
}
