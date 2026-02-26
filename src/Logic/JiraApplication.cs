using JiraReport.Abstractions;
using JiraReport.Models.Configuration;
using JiraReport.Models.ValueObjects;

using Microsoft.Extensions.Options;

namespace JiraReport.Logic;

internal sealed class JiraApplication : IJiraApplication
{
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
