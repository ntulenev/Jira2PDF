using System.Globalization;

using JiraReport.Abstractions;
using JiraReport.Models;
using JiraReport.Models.ValueObjects;

using Spectre.Console;

namespace JiraReport.Presentation;

/// <summary>
/// Spectre.Console based UI implementation.
/// </summary>
internal sealed class SpectreJiraPresentationService : IJiraPresentationService
{
    /// <inheritdoc />
    public ReportConfig? SelectReportConfig(IReadOnlyList<ReportConfig> sourceReports)
    {
        ArgumentNullException.ThrowIfNull(sourceReports);

        if (sourceReports.Count == 0)
        {
            return null;
        }

        var nameToConfig = sourceReports
            .OrderBy(static report => report.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static report => report.Name, static report => report, StringComparer.OrdinalIgnoreCase);

        var selectedName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select report config:")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                .AddChoices(nameToConfig.Keys));

        return nameToConfig[selectedName];
    }

    /// <inheritdoc />
    public string ResolvePdfPath(string defaultPdfPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultPdfPath);

        var selectedPath = AnsiConsole.Prompt(
            new TextPrompt<string>($"PDF output path (default: {Markup.Escape(defaultPdfPath)}):")
                .AllowEmpty())
            .Trim();

        selectedPath = string.IsNullOrWhiteSpace(selectedPath) ? defaultPdfPath : selectedPath;
        if (!selectedPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            selectedPath += ".pdf";
        }

        selectedPath = Path.GetFullPath(selectedPath);
        var directory = Path.GetDirectoryName(selectedPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }

        return selectedPath;
    }

    /// <inheritdoc />
    public void ShowReport(JiraJqlReport report, IReadOnlyList<OutputColumn> outputColumns)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(outputColumns);

        AnsiConsole.WriteLine();
        AnsiConsole.Write(
            new Rule($"[bold cyan]{Markup.Escape(report.Title)}[/]")
                .RuleStyle("grey")
                .LeftJustified());

        AnsiConsole.MarkupLine($"[grey]Generated:[/] {report.GeneratedAt:yyyy-MM-dd HH:mm:ss zzz}");
        AnsiConsole.MarkupLine($"[grey]Config:[/] {Markup.Escape(report.ConfigName)}");

        AnsiConsole.MarkupLine($"[grey]JQL:[/] {Markup.Escape(report.Jql)}");
        AnsiConsole.MarkupLine($"[grey]Total issues:[/] {report.Issues.Count.ToString(CultureInfo.InvariantCulture)}");

        foreach (var countTable in report.CountTables)
        {
            ShowCountTable(countTable.Title, countTable.Rows);
        }

        ShowIssuesTable(report.Issues, outputColumns);
    }

    /// <inheritdoc />
    public void ShowPdfSaved(string pdfPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pdfPath);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[green]PDF report saved to:[/] {Markup.Escape(pdfPath)}");
    }

    /// <inheritdoc />
    public void ShowError(ErrorMessage errorMessage) =>
        AnsiConsole.MarkupLine($"[red]Failed to generate Jira report:[/] {Markup.Escape(errorMessage.Value)}");

    /// <inheritdoc />
    public async Task<T> RunLoadingAsync<T>(string title, Func<Action<string>, Task<T>> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(action);

        return await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("cyan"))
            .StartAsync(Markup.Escape(title), async context =>
            {
                void UpdateStatus(string message)
                {
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        return;
                    }

                    _ = context.Status(Markup.Escape(message));
                }

                return await action(UpdateStatus).ConfigureAwait(false);
            })
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RunLoadingAsync(string title, Func<Action<string>, Task> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(action);

        _ = await RunLoadingAsync(
            title,
            async updateStatus =>
            {
                await action(updateStatus).ConfigureAwait(false);
                return true;
            }).ConfigureAwait(false);
    }

    private static void ShowCountTable(string title, IReadOnlyList<CountRow> counts)
    {
        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .AddColumn("[bold]Name[/]")
            .AddColumn(new TableColumn("[bold]Count[/]").RightAligned());

        foreach (var row in counts)
        {
            _ = table.AddRow(
                Markup.Escape(row.Name),
                row.Count.ToString(CultureInfo.InvariantCulture));
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold]{Markup.Escape(title)}[/]");
        AnsiConsole.Write(table);
    }

    private static void ShowIssuesTable(IReadOnlyList<JiraIssue> issues, IReadOnlyList<OutputColumn> outputColumns)
    {
        var table = new Table().RoundedBorder().BorderColor(Color.Grey);
        foreach (var outputColumn in outputColumns)
        {
            _ = table.AddColumn(
                new TableColumn($"[bold]{Markup.Escape(outputColumn.Header)}[/]")
                    .Width(ResolveConsoleWidth(outputColumn.Key)));
        }

        var shownIssuesCount = 0;
        foreach (var issue in issues.Take(CONSOLE_ISSUE_LIMIT))
        {
            _ = table.AddRow([.. outputColumns.Select(column =>
            {
                var value = column.Selector(issue);
                return string.IsNullOrWhiteSpace(value) ? "-" : Markup.Escape(value);
            })]);
            shownIssuesCount++;
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold]Issues[/]");
        AnsiConsole.Write(table);

        if (issues.Count > shownIssuesCount)
        {
            AnsiConsole.MarkupLine(
                $"[grey]Showing first {shownIssuesCount} issues in console. Full list is included in PDF.[/]");
        }
    }

    private static int ResolveConsoleWidth(string fieldKey) =>
        string.Equals(fieldKey, "summary", StringComparison.OrdinalIgnoreCase) ? 52 : 20;

    private const int CONSOLE_ISSUE_LIMIT = 50;
}
