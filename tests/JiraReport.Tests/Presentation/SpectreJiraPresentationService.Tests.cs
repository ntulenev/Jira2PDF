using FluentAssertions;

using JiraReport.Models;
using JiraReport.Models.ValueObjects;
using JiraReport.Presentation;

using Spectre.Console;
using Spectre.Console.Testing;

namespace JiraReport.Tests.Presentation;

public sealed class SpectreJiraPresentationServiceTests
{
    [Fact(DisplayName = "SelectReportConfig throws when source reports are null")]
    [Trait("Category", "Unit")]
    public void SelectReportConfigWhenSourceReportsAreNullThrowsArgumentNullException()
    {
        // Arrange
        var service = new SpectreJiraPresentationService();
        IReadOnlyList<ReportConfig> sourceReports = null!;

        // Act
        Action act = () => _ = service.SelectReportConfig(sourceReports);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "SelectReportConfig throws when source reports are empty")]
    [Trait("Category", "Unit")]
    public void SelectReportConfigWhenSourceReportsAreEmptyThrowsArgumentException()
    {
        // Arrange
        var service = new SpectreJiraPresentationService();

        // Act
        Action act = () => _ = service.SelectReportConfig([]);

        // Assert
        act.Should()
            .Throw<ArgumentException>();
    }

    [Fact(DisplayName = "SelectReportConfig returns alphabetically first report by default")]
    [Trait("Category", "Unit")]
    public async Task SelectReportConfigWhenPromptIsConfirmedReturnsAlphabeticallyFirstReport()
    {
        // Arrange
        var service = new SpectreJiraPresentationService();
        var reports = new[]
        {
            new ReportConfig(new ReportName("Zulu"), new JqlQuery("project = ZULU"), [], [], new PdfReportName("Zulu")),
            new ReportConfig(new ReportName("Alpha"), new JqlQuery("project = ALPHA"), [], [], new PdfReportName("Alpha"))
        };

        // Act
        var selected = await RunWithTestConsoleAsync(console =>
        {
            console.Input.PushKey(ConsoleKey.Enter);
            return Task.FromResult(service.SelectReportConfig(reports));
        });

        // Assert
        selected.Name.Value.Should().Be("Alpha");
    }

    [Fact(DisplayName = "ResolvePdfPath appends extension and returns full path")]
    [Trait("Category", "Unit")]
    public async Task ResolvePdfPathWhenCustomRelativePathIsEnteredAppendsExtensionAndReturnsFullPath()
    {
        // Arrange
        var service = new SpectreJiraPresentationService();

        // Act
        var path = await RunWithTestConsoleAsync(console =>
        {
            console.Input.PushTextWithEnter(@"reports\jira-output");
            return Task.FromResult(service.ResolvePdfPath(new PdfFilePath(@"C:\temp\default.pdf")));
        });

        // Assert
        Path.IsPathRooted(path.Value).Should().BeTrue();
        path.Value.EndsWith(@"reports\jira-output.pdf", StringComparison.OrdinalIgnoreCase).Should().BeTrue();
    }

    [Fact(DisplayName = "ShowReport writes report summary and issues table")]
    [Trait("Category", "Unit")]
    public async Task ShowReportWhenCalledWritesReportSummaryAndIssuesTable()
    {
        // Arrange
        var service = new SpectreJiraPresentationService();
        var report = new JiraJqlReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            new DateTimeOffset(2026, 2, 28, 18, 0, 0, TimeSpan.Zero),
            [new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue> { [new IssueKey("summary")] = new FieldValue("Implement report") })],
            [new CountTable("By Status", [new CountRow("Open", 1)])]);
        var outputColumns = new[]
        {
            new OutputColumn(IssueKey.DefaultKey, new OutputColumnHeader("Key"), static issue => issue.GetFieldValue(IssueKey.DefaultKey)),
            new OutputColumn(new IssueKey("summary"), new OutputColumnHeader("Summary"), static issue => issue.GetFieldValue(new IssueKey("summary")))
        };

        // Act
        var output = await CaptureOutputAsync(() =>
        {
            service.ShowReport(report, outputColumns);
            return Task.CompletedTask;
        });

        // Assert
        output.Should().Contain("Sprint report");
        output.Should().Contain("Backlog");
        output.Should().Contain("project = APP");
        output.Should().Contain("By Status");
        output.Should().Contain("Issues");
        output.Should().Contain("APP-1");
        output.Should().Contain("Implement report");
    }

    [Fact(DisplayName = "ShowPdfSaved writes PDF path message")]
    [Trait("Category", "Unit")]
    public async Task ShowPdfSavedWhenCalledWritesPdfPathMessage()
    {
        // Arrange
        var service = new SpectreJiraPresentationService();

        // Act
        var output = await CaptureOutputAsync(() =>
        {
            service.ShowPdfSaved(new PdfFilePath(@"C:\reports\jira.pdf"));
            return Task.CompletedTask;
        });

        // Assert
        output.Should().Contain(@"C:\reports\jira.pdf");
    }

    [Fact(DisplayName = "ShowError writes error message")]
    [Trait("Category", "Unit")]
    public async Task ShowErrorWhenCalledWritesErrorMessage()
    {
        // Arrange
        var service = new SpectreJiraPresentationService();

        // Act
        var output = await CaptureOutputAsync(() =>
        {
            service.ShowError(new ErrorMessage("Jira failed."));
            return Task.CompletedTask;
        });

        // Assert
        output.Should().Contain("Jira failed.");
    }

    [Fact(DisplayName = "RunLoadingAsync returns action result")]
    [Trait("Category", "Unit")]
    public async Task RunLoadingAsyncWhenActionSucceedsReturnsResult()
    {
        // Arrange
        var service = new SpectreJiraPresentationService();
        var updates = new List<string>();

        // Act
        var result = await RunWithTestConsoleAsync(async _ => await service.RunLoadingAsync(
            "Preparing report...",
            update =>
            {
                update("Loading issues...");
                updates.Add("Loading issues...");
                return Task.FromResult(42);
            }));

        // Assert
        result.Should().Be(42);
        updates.Should().ContainSingle().Which.Should().Be("Loading issues...");
    }

    [Fact(DisplayName = "RunLoadingAsync without result executes action")]
    [Trait("Category", "Unit")]
    public async Task RunLoadingAsyncWithoutResultWhenActionSucceedsExecutesAction()
    {
        // Arrange
        var service = new SpectreJiraPresentationService();
        var wasCalled = false;

        // Act
        await RunWithTestConsoleAsync(async _ =>
        {
            await service.RunLoadingAsync(
                "Preparing PDF...",
                update =>
                {
                    update("Rendering PDF...");
                    wasCalled = true;
                    return Task.CompletedTask;
                });

            return true;
        });

        // Assert
        wasCalled.Should().BeTrue();
    }

    private static async Task<T> RunWithTestConsoleAsync<T>(Func<TestConsole, Task<T>> action)
    {
        var original = AnsiConsole.Console;
        using var rawConsole = new TestConsole();
        var console = rawConsole.Interactive();
        AnsiConsole.Console = console;

        try
        {
            return await action(console);
        }
        finally
        {
            AnsiConsole.Console = original;
        }
    }

    private static async Task<string> CaptureOutputAsync(Func<Task> action)
    {
        var original = AnsiConsole.Console;
        using var rawConsole = new TestConsole();
        var console = rawConsole.Interactive();
        AnsiConsole.Console = console;

        try
        {
            await action();
            return console.Output;
        }
        finally
        {
            AnsiConsole.Console = original;
        }
    }
}
