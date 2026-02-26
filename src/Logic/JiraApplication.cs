using JiraReport.Abstractions;
using JiraReport.Models.Configuration;
using JiraReport.Models.ValueObjects;

using Microsoft.Extensions.Options;

namespace JiraReport.Logic;

/// <summary>
/// Orchestrates full report workflow from input to PDF output.
/// </summary>
internal sealed class JiraApplication : IJiraApplication
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JiraApplication"/> class.
    /// </summary>
    /// <param name="options">Application settings options.</param>
    /// <param name="args">Command line arguments.</param>
    /// <param name="jiraApiClient">Jira API client.</param>
    /// <param name="jiraLogicService">Domain logic service.</param>
    /// <param name="jiraPresentationService">Presentation service.</param>
    /// <param name="pdfReportRenderer">PDF report renderer.</param>
    public JiraApplication(
        IOptions<AppSettings> options,
        IReadOnlyList<string> args,
        IJiraApiClient jiraApiClient,
        IJiraLogicService jiraLogicService,
        IJiraPresentationService jiraPresentationService,
        IPdfReportRenderer pdfReportRenderer)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(jiraApiClient);
        ArgumentNullException.ThrowIfNull(jiraLogicService);
        ArgumentNullException.ThrowIfNull(jiraPresentationService);
        ArgumentNullException.ThrowIfNull(pdfReportRenderer);

        _settings = options.Value;
        _args = args;
        _jiraApiClient = jiraApiClient;
        _jiraLogicService = jiraLogicService;
        _jiraPresentationService = jiraPresentationService;
        _pdfReportRenderer = pdfReportRenderer;
    }

    /// <inheritdoc />
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var selectedReportConfig = _jiraPresentationService.SelectReportConfig(_settings.Reports);
            var jql = selectedReportConfig is null
                ? _jiraPresentationService.ResolveJql(_args)
                : selectedReportConfig.Jql;

            var outputColumns = _jiraLogicService.ResolveOutputColumns(selectedReportConfig?.OutputFields);
            var reportTitle = _jiraLogicService.ResolveReportTitle(selectedReportConfig);
            var defaultPdfPath = _jiraLogicService.BuildDefaultPdfPath(
                _settings.DefaultPdfPath,
                reportTitle,
                DateTimeOffset.Now);
            var outputPath = _jiraPresentationService.ResolvePdfPath(defaultPdfPath);

            var report = await _jiraPresentationService.RunLoadingAsync(
                "Preparing report...",
                async setLoadingStatus =>
                {
                    setLoadingStatus("Loading issues from Jira...");
                    var issues = await _jiraApiClient
                        .SearchIssuesAsync(jql, cancellationToken)
                        .ConfigureAwait(false);

                    setLoadingStatus("Building report data...");
                    return _jiraLogicService.BuildReport(
                        reportTitle,
                        selectedReportConfig?.Name,
                        jql,
                        issues);
                }).ConfigureAwait(false);

            _jiraPresentationService.ShowReport(report, outputColumns);

            await _jiraPresentationService.RunLoadingAsync(
                "Preparing PDF...",
                setLoadingStatus =>
                {
                    setLoadingStatus("Rendering PDF file...");
                    _pdfReportRenderer.RenderReport(report, _settings.BaseUrl, outputPath, outputColumns);
                    return Task.CompletedTask;
                }).ConfigureAwait(false);

            _jiraPresentationService.ShowPdfSaved(outputPath);
        }
        catch (HttpRequestException ex)
        {
            _jiraPresentationService.ShowError(ErrorMessage.FromException(ex));
        }
        catch (InvalidOperationException ex)
        {
            _jiraPresentationService.ShowError(ErrorMessage.FromException(ex));
        }
        catch (System.Text.Json.JsonException ex)
        {
            _jiraPresentationService.ShowError(ErrorMessage.FromException(ex));
        }
        catch (IOException ex)
        {
            _jiraPresentationService.ShowError(ErrorMessage.FromException(ex));
        }
    }

    private readonly AppSettings _settings;
    private readonly IReadOnlyList<string> _args;
    private readonly IJiraApiClient _jiraApiClient;
    private readonly IJiraLogicService _jiraLogicService;
    private readonly IJiraPresentationService _jiraPresentationService;
    private readonly IPdfReportRenderer _pdfReportRenderer;
}
