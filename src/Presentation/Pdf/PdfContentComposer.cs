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

        if (report.FlowPathGroups.Count > 0)
        {
            column.Item().PageBreak();
            _ = column.Item().Text("Workflow transitions").Bold().FontSize(14);
            foreach (var group in report.FlowPathGroups)
            {
                ComposeFlowPathGroup(column, group, baseUrl);
            }
        }

        _ = column.Item().Text("Issues").Bold();
        column.Item().Element(container => ComposeIssuesTable(container, report.Issues, outputColumns, baseUrl));
    }

    private static void ComposeFlowPathGroup(ColumnDescriptor column, FlowPathGroup group, JiraBaseUrl baseUrl)
    {
        _ = column.Item().Text($"{group.Path} ({group.Issues.Count} issue(s))").Bold();
        column.Item().Text(text =>
        {
            _ = text.Span("Issues: ");
            foreach (var (key, index) in group.Issues.Select((key, index) => (key, index)))
            {
                if (index > 0)
                {
                    _ = text.Span(", ");
                }
                _ = text.Hyperlink(key.Value, PdfPresentationHelpers.BuildIssueBrowseUrl(baseUrl, key))
                    .FontColor(Colors.Blue.Darken2).Underline();
            }
        });

        var weights = group.Stages.Select(static stage => Math.Max(1d, stage.MedianDuration.TotalHours)).ToList();
        column.Item().Height(18).Row(row =>
        {
            for (var index = 0; index < group.Stages.Count; index++)
            {
                var color = _flowColors[index % _flowColors.Length];
                _ = row.RelativeItem((float)weights[index]).Background(color).Padding(2)
                    .AlignMiddle().Text(group.Stages[index].Status).FontSize(7).FontColor(Colors.White);
            }
        });

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1);
            });
            table.Header(header =>
            {
                _ = header.Cell().Element(PdfPresentationHelpers.StyleHeaderCell).Text("Status");
                _ = header.Cell().Element(PdfPresentationHelpers.StyleHeaderCell).Text("Next status");
                _ = header.Cell().Element(PdfPresentationHelpers.StyleHeaderCell).Text("Median time");
            });
            foreach (var stage in group.Stages)
            {
                _ = table.Cell().Element(PdfPresentationHelpers.StyleBodyCell).Text(stage.Status);
                _ = table.Cell().Element(PdfPresentationHelpers.StyleBodyCell).Text(stage.NextStatus);
                _ = table.Cell().Element(PdfPresentationHelpers.StyleBodyCell).Text(FormatDuration(stage.MedianDuration));
            }
        });
    }

    private static string FormatDuration(TimeSpan duration) =>
        duration.TotalDays >= 1
            ? $"{(int)duration.TotalDays}d {duration.Hours}h"
            : $"{duration.TotalHours:0.#}h";

    private static readonly string[] _flowColors =
        ["#2563eb", "#7c3aed", "#db2777", "#ea580c", "#16a34a", "#0891b2", "#4f46e5"];

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
                    if (column.Key == new IssueKey("summary"))
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
                    _ = header.Cell().Element(PdfPresentationHelpers.StyleHeaderCell).Text(column.Header.Value);
                }
            });

            foreach (var issue in issues)
            {
                foreach (var column in outputColumns)
                {
                    if (column.Key == IssueKey.DefaultKey)
                    {
                        var issueUrl = PdfPresentationHelpers.BuildIssueBrowseUrl(baseUrl, issue);
                        _ = table.Cell()
                            .Element(PdfPresentationHelpers.StyleBodyCell)
                            .Hyperlink(issueUrl)
                            .DefaultTextStyle(static style => style.FontColor(Colors.Blue.Darken2).Underline())
                            .Text(issue.Key.Value);
                    }
                    else
                    {
                        var value = column.Selector(issue).Value;
                        _ = table.Cell().Element(PdfPresentationHelpers.StyleBodyCell).Text(value);
                    }
                }
            }
        });
    }
}
