using System.Globalization;

using JiraReport.Abstractions;
using JiraReport.Models;
using JiraReport.Models.ValueObjects;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JiraReport.Presentation.Pdf;

/// <summary>
/// Composes PDF report content sections.
/// </summary>
internal sealed class PdfContentComposer : IPdfContentComposer
{
    /// <inheritdoc />
    public void ComposeContent(
        ColumnDescriptor column,
        JiraJqlReport report,
        IReadOnlyList<OutputColumn> outputColumns,
        JiraBaseUrl baseUrl)
    {
        ArgumentNullException.ThrowIfNull(column);
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(outputColumns);

        column.Spacing(10);

        foreach (var countTable in report.CountTables)
        {
            _ = column.Item().Text($"Summary {countTable.Title}").Bold();
            column.Item().Element(container => ComposeCountTable(container, countTable.Rows));
        }

        _ = column.Item().Text("Issues").Bold();
        column.Item().Element(container => ComposeIssuesTable(container, report.Issues, outputColumns, baseUrl));
    }

    private static void ComposeCountTable(IContainer container, IReadOnlyList<CountRow> counts)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.RelativeColumn(1);
            });

            table.Header(header =>
            {
                _ = header.Cell().Element(PdfPresentationHelpers.StyleHeaderCell).Text("Name");
                _ = header.Cell().Element(PdfPresentationHelpers.StyleHeaderCell).AlignRight().Text("Count");
            });

            foreach (var row in counts)
            {
                _ = table.Cell().Element(PdfPresentationHelpers.StyleBodyCell).Text(row.Name);
                _ = table.Cell()
                    .Element(PdfPresentationHelpers.StyleBodyCell)
                    .AlignRight()
                    .Text(row.Count.ToString(CultureInfo.InvariantCulture));
            }
        });
    }

    private static void ComposeIssuesTable(
        IContainer container,
        IReadOnlyList<JiraIssue> issues,
        IReadOnlyList<OutputColumn> outputColumns,
        JiraBaseUrl baseUrl)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                foreach (var column in outputColumns)
                {
                    if (string.Equals(column.Key, "summary", StringComparison.OrdinalIgnoreCase))
                    {
                        columns.RelativeColumn(3);
                    }
                    else
                    {
                        columns.RelativeColumn(1);
                    }
                }
            });

            table.Header(header =>
            {
                foreach (var column in outputColumns)
                {
                    _ = header.Cell().Element(PdfPresentationHelpers.StyleHeaderCell).Text(column.Header);
                }
            });

            foreach (var issue in issues)
            {
                foreach (var column in outputColumns)
                {
                    if (string.Equals(column.Key, "key", StringComparison.OrdinalIgnoreCase))
                    {
                        var issueUrl = PdfPresentationHelpers.BuildIssueBrowseUrl(baseUrl, issue);
                        _ = table.Cell()
                            .Element(PdfPresentationHelpers.StyleBodyCell)
                            .Hyperlink(issueUrl)
                            .DefaultTextStyle(static style => style.FontColor(Colors.Blue.Darken2).Underline())
                            .Text(issue.Key);
                    }
                    else
                    {
                        var value = column.Selector(issue);
                        _ = table.Cell().Element(PdfPresentationHelpers.StyleBodyCell).Text(value);
                    }
                }
            }
        });
    }
}
