using System.Globalization;

using JiraReport.Models;
using JiraReport.Models.ValueObjects;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JiraReport.Presentation.Pdf;

/// <summary>
/// Shared helper methods for PDF presentation layer.
/// </summary>
internal static class PdfPresentationHelpers
{
    /// <summary>
    /// Applies standard header cell styling.
    /// </summary>
    /// <param name="container">Target container.</param>
    /// <returns>Styled container.</returns>
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

    /// <summary>
    /// Applies standard body cell styling.
    /// </summary>
    /// <param name="container">Target container.</param>
    /// <returns>Styled container.</returns>
    public static IContainer StyleBodyCell(IContainer container)
    {
        ArgumentNullException.ThrowIfNull(container);
        return container
            .Border(0.5f)
            .PaddingVertical(3)
            .PaddingHorizontal(3);
    }

    /// <summary>
    /// Builds Jira issue browse URL for an issue model.
    /// </summary>
    /// <param name="baseUrl">Jira base URL.</param>
    /// <param name="issue">Issue model.</param>
    /// <returns>Issue browse URL.</returns>
    public static string BuildIssueBrowseUrl(JiraBaseUrl baseUrl, JiraIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);
        return BuildIssueBrowseUrl(baseUrl, issue.Key);
    }

    /// <summary>
    /// Builds Jira issue browse URL for an issue key.
    /// </summary>
    /// <param name="baseUrl">Jira base URL.</param>
    /// <param name="issueKey">Issue key.</param>
    /// <returns>Issue browse URL.</returns>
    public static string BuildIssueBrowseUrl(JiraBaseUrl baseUrl, string issueKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(issueKey);

        var trimmedBaseUrl = baseUrl.Value.TrimEnd('/');
        var escapedIssueKey = Uri.EscapeDataString(issueKey.Trim());
        return $"{trimmedBaseUrl}/browse/{escapedIssueKey}";
    }

    /// <summary>
    /// Formats optional timestamp as date-only text.
    /// </summary>
    /// <param name="value">Optional timestamp value.</param>
    /// <returns>Date text in <c>yyyy-MM-dd</c> format or dash.</returns>
    public static string ToDateOnly(DateTimeOffset? value) =>
        value.HasValue
            ? value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : "-";
}
