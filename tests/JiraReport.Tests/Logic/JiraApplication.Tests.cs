using FluentAssertions;

using JiraReport.Abstractions;
using JiraReport.Logic;
using JiraReport.Models;
using JiraReport.Models.Configuration;
using JiraReport.Models.ValueObjects;

using Microsoft.Extensions.Options;

using Moq;

namespace JiraReport.Tests.Logic;

public sealed class JiraApplicationTests
{
    [Fact(DisplayName = "Constructor throws when options are null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenOptionsAreNullThrowsArgumentNullException()
    {
        // Arrange
        IOptions<AppSettings> options = null!;
        var jiraApiClient = new Mock<IJiraApiClient>(MockBehavior.Strict).Object;
        var jiraLogicService = new Mock<IJiraLogicService>(MockBehavior.Strict).Object;
        var jiraPresentationService = new Mock<IJiraPresentationService>(MockBehavior.Strict).Object;
        var pdfReportRenderer = new Mock<IPdfReportRenderer>(MockBehavior.Strict).Object;
        var pdfReportLauncher = new Mock<IPdfReportLauncher>(MockBehavior.Strict).Object;
        var csvReportWriter = new Mock<ICsvReportWriter>(MockBehavior.Strict).Object;

        // Act
        Action act = () => _ = new JiraApplication(options, jiraApiClient, jiraLogicService, jiraPresentationService, pdfReportRenderer, pdfReportLauncher, csvReportWriter);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when Jira API client is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenJiraApiClientIsNullThrowsArgumentNullException()
    {
        // Arrange
        IJiraApiClient jiraApiClient = null!;
        var options = Options.Create(CreateSettings());
        var jiraLogicService = new Mock<IJiraLogicService>(MockBehavior.Strict).Object;
        var jiraPresentationService = new Mock<IJiraPresentationService>(MockBehavior.Strict).Object;
        var pdfReportRenderer = new Mock<IPdfReportRenderer>(MockBehavior.Strict).Object;
        var pdfReportLauncher = new Mock<IPdfReportLauncher>(MockBehavior.Strict).Object;
        var csvReportWriter = new Mock<ICsvReportWriter>(MockBehavior.Strict).Object;

        // Act
        Action act = () => _ = new JiraApplication(options, jiraApiClient, jiraLogicService, jiraPresentationService, pdfReportRenderer, pdfReportLauncher, csvReportWriter);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when Jira logic service is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenJiraLogicServiceIsNullThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(CreateSettings());
        var jiraApiClient = new Mock<IJiraApiClient>(MockBehavior.Strict).Object;
        IJiraLogicService jiraLogicService = null!;
        var jiraPresentationService = new Mock<IJiraPresentationService>(MockBehavior.Strict).Object;
        var pdfReportRenderer = new Mock<IPdfReportRenderer>(MockBehavior.Strict).Object;
        var pdfReportLauncher = new Mock<IPdfReportLauncher>(MockBehavior.Strict).Object;
        var csvReportWriter = new Mock<ICsvReportWriter>(MockBehavior.Strict).Object;

        // Act
        Action act = () => _ = new JiraApplication(options, jiraApiClient, jiraLogicService, jiraPresentationService, pdfReportRenderer, pdfReportLauncher, csvReportWriter);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when Jira presentation service is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenJiraPresentationServiceIsNullThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(CreateSettings());
        var jiraApiClient = new Mock<IJiraApiClient>(MockBehavior.Strict).Object;
        var jiraLogicService = new Mock<IJiraLogicService>(MockBehavior.Strict).Object;
        IJiraPresentationService jiraPresentationService = null!;
        var pdfReportRenderer = new Mock<IPdfReportRenderer>(MockBehavior.Strict).Object;
        var pdfReportLauncher = new Mock<IPdfReportLauncher>(MockBehavior.Strict).Object;
        var csvReportWriter = new Mock<ICsvReportWriter>(MockBehavior.Strict).Object;

        // Act
        Action act = () => _ = new JiraApplication(options, jiraApiClient, jiraLogicService, jiraPresentationService, pdfReportRenderer, pdfReportLauncher, csvReportWriter);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when PDF report renderer is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenPdfReportRendererIsNullThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(CreateSettings());
        var jiraApiClient = new Mock<IJiraApiClient>(MockBehavior.Strict).Object;
        var jiraLogicService = new Mock<IJiraLogicService>(MockBehavior.Strict).Object;
        var jiraPresentationService = new Mock<IJiraPresentationService>(MockBehavior.Strict).Object;
        IPdfReportRenderer pdfReportRenderer = null!;
        var pdfReportLauncher = new Mock<IPdfReportLauncher>(MockBehavior.Strict).Object;
        var csvReportWriter = new Mock<ICsvReportWriter>(MockBehavior.Strict).Object;

        // Act
        Action act = () => _ = new JiraApplication(options, jiraApiClient, jiraLogicService, jiraPresentationService, pdfReportRenderer, pdfReportLauncher, csvReportWriter);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when CSV report writer is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenCsvReportWriterIsNullThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(CreateSettings());
        var jiraApiClient = new Mock<IJiraApiClient>(MockBehavior.Strict).Object;
        var jiraLogicService = new Mock<IJiraLogicService>(MockBehavior.Strict).Object;
        var jiraPresentationService = new Mock<IJiraPresentationService>(MockBehavior.Strict).Object;
        var pdfReportRenderer = new Mock<IPdfReportRenderer>(MockBehavior.Strict).Object;
        var pdfReportLauncher = new Mock<IPdfReportLauncher>(MockBehavior.Strict).Object;
        ICsvReportWriter csvReportWriter = null!;

        // Act
        Action act = () => _ = new JiraApplication(options, jiraApiClient, jiraLogicService, jiraPresentationService, pdfReportRenderer, pdfReportLauncher, csvReportWriter);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Constructor throws when PDF report launcher is null")]
    [Trait("Category", "Unit")]
    public void ConstructorWhenPdfReportLauncherIsNullThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(CreateSettings());
        var jiraApiClient = new Mock<IJiraApiClient>(MockBehavior.Strict).Object;
        var jiraLogicService = new Mock<IJiraLogicService>(MockBehavior.Strict).Object;
        var jiraPresentationService = new Mock<IJiraPresentationService>(MockBehavior.Strict).Object;
        var pdfReportRenderer = new Mock<IPdfReportRenderer>(MockBehavior.Strict).Object;
        IPdfReportLauncher pdfReportLauncher = null!;
        var csvReportWriter = new Mock<ICsvReportWriter>(MockBehavior.Strict).Object;

        // Act
        Action act = () => _ = new JiraApplication(options, jiraApiClient, jiraLogicService, jiraPresentationService, pdfReportRenderer, pdfReportLauncher, csvReportWriter);

        // Assert
        act.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "RunAsync orchestrates report workflow and renders PDF")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncWhenWorkflowSucceedsShowsReportAndRendersPdf()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var selectedReport = new ReportConfig(
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            [new IssueFieldName("summary")],
            [new IssueFieldName("status")],
            new PdfReportName("Sprint report"));
        var outputColumns = new[] { new OutputColumn(new IssueKey("summary"), new OutputColumnHeader("Summary"), static issue => issue.GetFieldValue(new IssueKey("summary"))) };
        var requestedFields = new[] { new IssueFieldName("summary"), new IssueFieldName("status") };
        var defaultPdfPath = new PdfFilePath(@"C:\reports\default.pdf");
        var outputPath = new PdfFilePath(@"C:\reports\resolved.pdf");
        var issues = new[] { new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue>()) };
        var report = new JiraJqlReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            new DateTimeOffset(2026, 2, 28, 18, 0, 0, TimeSpan.Zero),
            issues,
            []);

        var jiraApiClient = new Mock<IJiraApiClient>(MockBehavior.Strict);
        jiraApiClient.Setup(client => client.SearchIssuesAsync(selectedReport.Jql, requestedFields, cts.Token))
            .ReturnsAsync(issues);

        var jiraLogicService = new Mock<IJiraLogicService>(MockBehavior.Strict);
        jiraLogicService.Setup(service => service.ResolveOutputColumns(selectedReport.OutputFields))
            .Returns(outputColumns);
        jiraLogicService.Setup(service => service.ResolveRequestedIssueFields(selectedReport.OutputFields, selectedReport.CountFields))
            .Returns(requestedFields);
        jiraLogicService.Setup(service => service.BuildDefaultPdfPath(selectedReport.PdfReportName, It.IsAny<DateTimeOffset>()))
            .Returns(defaultPdfPath);
        jiraLogicService.Setup(service => service.BuildReport(
                selectedReport.PdfReportName,
                selectedReport.Name,
                selectedReport.Jql,
                issues,
                selectedReport.CountFields))
            .Returns(report);

        var jiraPresentationService = new Mock<IJiraPresentationService>(MockBehavior.Strict);
        jiraPresentationService.Setup(service => service.SelectReportConfig(It.Is<IReadOnlyList<ReportConfig>>(reports => reports.Count == 1)))
            .Returns(selectedReport);
        jiraPresentationService.Setup(service => service.ResolvePdfPath(defaultPdfPath))
            .Returns(outputPath);
        jiraPresentationService.Setup(service => service.RunLoadingAsync(
                "Preparing report...",
                It.IsAny<Func<Action<string>, Task<JiraJqlReport>>>()))
            .Returns<string, Func<Action<string>, Task<JiraJqlReport>>>((_, action) => action(static _ => { }));
        jiraPresentationService.Setup(service => service.ShowReport(report, outputColumns));
        jiraPresentationService.Setup(service => service.RunLoadingAsync(
                "Preparing PDF...",
                It.IsAny<Func<Action<string>, Task>>()))
            .Returns<string, Func<Action<string>, Task>>((_, action) => action(static _ => { }));
        jiraPresentationService.Setup(service => service.ShowPdfSaved(outputPath));

        var pdfReportRenderer = new Mock<IPdfReportRenderer>(MockBehavior.Strict);
        pdfReportRenderer.Setup(renderer => renderer.RenderReport(report, CreateSettings().BaseUrl, outputPath, outputColumns));

        var pdfReportLauncher = new Mock<IPdfReportLauncher>(MockBehavior.Strict);
        var csvReportWriter = new Mock<ICsvReportWriter>(MockBehavior.Strict);

        var app = new JiraApplication(
            Options.Create(CreateSettings()),
            jiraApiClient.Object,
            jiraLogicService.Object,
            jiraPresentationService.Object,
            pdfReportRenderer.Object,
            pdfReportLauncher.Object,
            csvReportWriter.Object);

        // Act
        await app.RunAsync(cts.Token);

        // Assert
        jiraApiClient.VerifyAll();
        jiraLogicService.VerifyAll();
        jiraPresentationService.VerifyAll();
        pdfReportRenderer.VerifyAll();
        csvReportWriter.VerifyAll();
    }

    [Fact(DisplayName = "RunAsync writes companion CSV when enabled")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncWhenCsvIsEnabledWritesCsv()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var selectedReport = new ReportConfig(
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            [new IssueFieldName("summary")],
            [],
            new PdfReportName("Sprint report"));
        var outputColumns = new[] { new OutputColumn(new IssueKey("summary"), new OutputColumnHeader("Summary"), static issue => issue.GetFieldValue(new IssueKey("summary"))) };
        var requestedFields = new[] { new IssueFieldName("summary") };
        var defaultPdfPath = new PdfFilePath(@"C:\reports\default.pdf");
        var outputPath = new PdfFilePath(@"C:\reports\resolved.pdf");
        var csvPath = new CsvFilePath(@"C:\reports\resolved_raw.csv");
        var issues = new[] { new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue> { [new IssueKey("summary")] = new FieldValue("Implement report") }) };
        var report = new JiraJqlReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            new DateTimeOffset(2026, 2, 28, 18, 0, 0, TimeSpan.Zero),
            issues,
            []);

        var jiraApiClient = new Mock<IJiraApiClient>(MockBehavior.Strict);
        jiraApiClient.Setup(client => client.SearchIssuesAsync(selectedReport.Jql, requestedFields, cts.Token))
            .ReturnsAsync(issues);

        var jiraLogicService = new Mock<IJiraLogicService>(MockBehavior.Strict);
        jiraLogicService.Setup(service => service.ResolveOutputColumns(selectedReport.OutputFields))
            .Returns(outputColumns);
        jiraLogicService.Setup(service => service.ResolveRequestedIssueFields(selectedReport.OutputFields, selectedReport.CountFields))
            .Returns(requestedFields);
        jiraLogicService.Setup(service => service.BuildDefaultPdfPath(selectedReport.PdfReportName, It.IsAny<DateTimeOffset>()))
            .Returns(defaultPdfPath);
        jiraLogicService.Setup(service => service.BuildReport(
                selectedReport.PdfReportName,
                selectedReport.Name,
                selectedReport.Jql,
                issues,
                selectedReport.CountFields))
            .Returns(report);

        var jiraPresentationService = new Mock<IJiraPresentationService>(MockBehavior.Strict);
        jiraPresentationService.Setup(service => service.SelectReportConfig(It.IsAny<IReadOnlyList<ReportConfig>>()))
            .Returns(selectedReport);
        jiraPresentationService.Setup(service => service.ResolvePdfPath(defaultPdfPath))
            .Returns(outputPath);
        jiraPresentationService.Setup(service => service.RunLoadingAsync(
                "Preparing report...",
                It.IsAny<Func<Action<string>, Task<JiraJqlReport>>>()))
            .Returns<string, Func<Action<string>, Task<JiraJqlReport>>>((_, action) => action(static _ => { }));
        jiraPresentationService.Setup(service => service.ShowReport(report, outputColumns));
        jiraPresentationService.Setup(service => service.RunLoadingAsync(
                "Preparing PDF...",
                It.IsAny<Func<Action<string>, Task>>()))
            .Returns<string, Func<Action<string>, Task>>((_, action) => action(static _ => { }));
        jiraPresentationService.Setup(service => service.ShowPdfSaved(outputPath));
        jiraPresentationService.Setup(service => service.RunLoadingAsync(
                "Preparing CSV...",
                It.IsAny<Func<Action<string>, Task>>()))
            .Returns<string, Func<Action<string>, Task>>((_, action) => action(static _ => { }));
        jiraPresentationService.Setup(service => service.ShowCsvSaved(csvPath));

        var pdfReportRenderer = new Mock<IPdfReportRenderer>(MockBehavior.Strict);
        pdfReportRenderer.Setup(renderer => renderer.RenderReport(report, CreateSettings(false, true, true).BaseUrl, outputPath, outputColumns));

        var pdfReportLauncher = new Mock<IPdfReportLauncher>(MockBehavior.Strict);
        var csvReportWriter = new Mock<ICsvReportWriter>(MockBehavior.Strict);
        csvReportWriter.Setup(writer => writer.WriteReportAsync(report, csvPath, outputColumns, true))
            .Returns(Task.CompletedTask);

        var app = new JiraApplication(
            Options.Create(CreateSettings(csvEnabled: true, displayHeaders: true)),
            jiraApiClient.Object,
            jiraLogicService.Object,
            jiraPresentationService.Object,
            pdfReportRenderer.Object,
            pdfReportLauncher.Object,
            csvReportWriter.Object);

        // Act
        await app.RunAsync(cts.Token);

        // Assert
        jiraApiClient.VerifyAll();
        jiraLogicService.VerifyAll();
        jiraPresentationService.VerifyAll();
        pdfReportRenderer.VerifyAll();
        csvReportWriter.VerifyAll();
    }

    [Fact(DisplayName = "RunAsync opens PDF when configured")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncWhenPdfAutoOpenIsEnabledOpensGeneratedPdf()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var selectedReport = new ReportConfig(
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            [new IssueFieldName("summary")],
            [],
            new PdfReportName("Sprint report"));
        var outputColumns = new[] { new OutputColumn(new IssueKey("summary"), new OutputColumnHeader("Summary"), static issue => issue.GetFieldValue(new IssueKey("summary"))) };
        var requestedFields = new[] { new IssueFieldName("summary") };
        var defaultPdfPath = new PdfFilePath(@"C:\reports\default.pdf");
        var outputPath = new PdfFilePath(@"C:\reports\resolved.pdf");
        var issues = new[] { new JiraIssue(new IssueKey("APP-1"), new Dictionary<IssueKey, FieldValue>()) };
        var report = new JiraJqlReport(
            new PdfReportName("Sprint report"),
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            new DateTimeOffset(2026, 2, 28, 18, 0, 0, TimeSpan.Zero),
            issues,
            []);

        var jiraApiClient = new Mock<IJiraApiClient>(MockBehavior.Strict);
        jiraApiClient.Setup(client => client.SearchIssuesAsync(selectedReport.Jql, requestedFields, cts.Token))
            .ReturnsAsync(issues);

        var jiraLogicService = new Mock<IJiraLogicService>(MockBehavior.Strict);
        jiraLogicService.Setup(service => service.ResolveOutputColumns(selectedReport.OutputFields))
            .Returns(outputColumns);
        jiraLogicService.Setup(service => service.ResolveRequestedIssueFields(selectedReport.OutputFields, selectedReport.CountFields))
            .Returns(requestedFields);
        jiraLogicService.Setup(service => service.BuildDefaultPdfPath(selectedReport.PdfReportName, It.IsAny<DateTimeOffset>()))
            .Returns(defaultPdfPath);
        jiraLogicService.Setup(service => service.BuildReport(
                selectedReport.PdfReportName,
                selectedReport.Name,
                selectedReport.Jql,
                issues,
                selectedReport.CountFields))
            .Returns(report);

        var jiraPresentationService = new Mock<IJiraPresentationService>(MockBehavior.Strict);
        jiraPresentationService.Setup(service => service.SelectReportConfig(It.IsAny<IReadOnlyList<ReportConfig>>()))
            .Returns(selectedReport);
        jiraPresentationService.Setup(service => service.ResolvePdfPath(defaultPdfPath))
            .Returns(outputPath);
        jiraPresentationService.Setup(service => service.RunLoadingAsync(
                "Preparing report...",
                It.IsAny<Func<Action<string>, Task<JiraJqlReport>>>()))
            .Returns<string, Func<Action<string>, Task<JiraJqlReport>>>((_, action) => action(static _ => { }));
        jiraPresentationService.Setup(service => service.ShowReport(report, outputColumns));
        jiraPresentationService.Setup(service => service.RunLoadingAsync(
                "Preparing PDF...",
                It.IsAny<Func<Action<string>, Task>>()))
            .Returns<string, Func<Action<string>, Task>>((_, action) => action(static _ => { }));
        jiraPresentationService.Setup(service => service.ShowPdfSaved(outputPath));

        var pdfReportRenderer = new Mock<IPdfReportRenderer>(MockBehavior.Strict);
        pdfReportRenderer.Setup(renderer => renderer.RenderReport(report, CreateSettings(true).BaseUrl, outputPath, outputColumns));

        var pdfReportLauncher = new Mock<IPdfReportLauncher>(MockBehavior.Strict);
        pdfReportLauncher.Setup(launcher => launcher.Open(outputPath));

        var csvReportWriter = new Mock<ICsvReportWriter>(MockBehavior.Strict);

        var app = new JiraApplication(
            Options.Create(CreateSettings(pdfOpenAfterGeneration: true)),
            jiraApiClient.Object,
            jiraLogicService.Object,
            jiraPresentationService.Object,
            pdfReportRenderer.Object,
            pdfReportLauncher.Object,
            csvReportWriter.Object);

        // Act
        await app.RunAsync(cts.Token);

        // Assert
        jiraApiClient.VerifyAll();
        jiraLogicService.VerifyAll();
        jiraPresentationService.VerifyAll();
        pdfReportRenderer.VerifyAll();
        pdfReportLauncher.VerifyAll();
        csvReportWriter.VerifyAll();
    }

    [Fact(DisplayName = "RunAsync shows error when Jira API request fails")]
    [Trait("Category", "Unit")]
    public async Task RunAsyncWhenHttpRequestFailsShowsError()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var selectedReport = new ReportConfig(
            new ReportName("Backlog"),
            new JqlQuery("project = APP"),
            [],
            [],
            new PdfReportName("Sprint report"));

        var jiraApiClient = new Mock<IJiraApiClient>(MockBehavior.Strict);
        jiraApiClient.Setup(client => client.SearchIssuesAsync(selectedReport.Jql, It.IsAny<IReadOnlyList<IssueFieldName>>(), cts.Token))
            .ThrowsAsync(new HttpRequestException("Jira failed."));

        var jiraLogicService = new Mock<IJiraLogicService>(MockBehavior.Strict);
        jiraLogicService.Setup(service => service.ResolveOutputColumns(selectedReport.OutputFields))
            .Returns(Array.Empty<OutputColumn>());
        jiraLogicService.Setup(service => service.ResolveRequestedIssueFields(selectedReport.OutputFields, selectedReport.CountFields))
            .Returns(Array.Empty<IssueFieldName>());
        jiraLogicService.Setup(service => service.BuildDefaultPdfPath(selectedReport.PdfReportName, It.IsAny<DateTimeOffset>()))
            .Returns(new PdfFilePath(@"C:\reports\default.pdf"));

        var jiraPresentationService = new Mock<IJiraPresentationService>(MockBehavior.Strict);
        jiraPresentationService.Setup(service => service.SelectReportConfig(It.IsAny<IReadOnlyList<ReportConfig>>()))
            .Returns(selectedReport);
        jiraPresentationService.Setup(service => service.ResolvePdfPath(It.IsAny<PdfFilePath>()))
            .Returns(new PdfFilePath(@"C:\reports\resolved.pdf"));
        jiraPresentationService.Setup(service => service.RunLoadingAsync(
                "Preparing report...",
                It.IsAny<Func<Action<string>, Task<JiraJqlReport>>>()))
            .Returns<string, Func<Action<string>, Task<JiraJqlReport>>>((_, action) => action(static _ => { }));
        jiraPresentationService.Setup(service => service.ShowError(It.Is<ErrorMessage>(error => error.Value == "Jira failed.")));

        var pdfReportRenderer = new Mock<IPdfReportRenderer>(MockBehavior.Strict);
        var pdfReportLauncher = new Mock<IPdfReportLauncher>(MockBehavior.Strict);
        var csvReportWriter = new Mock<ICsvReportWriter>(MockBehavior.Strict);

        var app = new JiraApplication(
            Options.Create(CreateSettings()),
            jiraApiClient.Object,
            jiraLogicService.Object,
            jiraPresentationService.Object,
            pdfReportRenderer.Object,
            pdfReportLauncher.Object,
            csvReportWriter.Object);

        // Act
        await app.RunAsync(cts.Token);

        // Assert
        jiraPresentationService.VerifyAll();
    }

    private static AppSettings CreateSettings(bool pdfOpenAfterGeneration = false, bool csvEnabled = false, bool displayHeaders = false)
    {
        return new AppSettings(
            new JiraBaseUrl("https://example.test"),
            new JiraEmail("user@example.test"),
            new JiraApiToken("token"),
            50,
            0,
            [new ReportConfig(new ReportName("Backlog"), new JqlQuery("project = APP"), [], [], new PdfReportName("Sprint report"))],
            new PdfSettings(pdfOpenAfterGeneration),
            new CsvSettings(csvEnabled, displayHeaders));
    }
}
