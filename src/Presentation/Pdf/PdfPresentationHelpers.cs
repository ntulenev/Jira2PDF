using System.Globalization;

using JiraReport.Models;
using JiraReport.Models.ValueObjects;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JiraReport.Presentation.Pdf;

internal static class PdfPresentationHelpers
{
    public static IContainer StyleHeaderCell(IContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);
        return container
            .Border(0.5f)
            .Background(Colors.Grey.Lighten2)
            .PaddingVertical(4)
            .PaddingHorizontal(3)
            .DefaultTextStyle(static style => style.SemiBold());
    }

    public static IContainer StyleBodyCell(IContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);
        return container
            .Border(0.5f)
            .PaddingVertical(3)
            .PaddingHorizontal(3);
    }

    public static string BuildIssueBrowseUrl(JiraBaseUrl baseUrl, JiraIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);
        return BuildIssueBrowseUrl(baseUrl, issue.Key);
    }

    public static string BuildIssueBrowseUrl(JiraBaseUrl baseUrl, string issueKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issueKey);

        var trimmedBaseUrl = baseUrl.Value.TrimEnd('/');
        var escapedIssueKey = Uri.EscapeDataString(issueKey.Trim());
        return $"{trimmedBaseUrl}/browse/{escapedIssueKey}";
    }

    public static string ToDateOnly(DateTimeOffset? value) =>
        value.HasValue
            ? value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : "-";
}
