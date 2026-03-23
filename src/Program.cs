using System.Net.Http.Headers;
using System.Text;

using JiraReport.Abstractions;
using JiraReport.API;
using JiraReport.API.Mapping;
using JiraReport.Logic;
using JiraReport.Models;
using JiraReport.Models.Configuration;
using JiraReport.Models.ValueObjects;
using JiraReport.Presentation;
using JiraReport.Presentation.Csv;
using JiraReport.Presentation.Pdf;
using JiraReport.Transport;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

Console.OutputEncoding = Encoding.UTF8;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);

builder.Services
    .AddOptions<JiraOptions>()
    .Bind(builder.Configuration.GetSection("Jira"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<CsvOptions>()
    .Bind(builder.Configuration.GetSection("CSV"))
    .ValidateOnStart();

builder.Services
    .AddOptions<UiOptions>()
    .Bind(builder.Configuration.GetSection("UI"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<PdfOptions>()
    .Bind(builder.Configuration.GetSection("PDF"))
    .ValidateOnStart();

builder.Services.AddSingleton(sp =>
{
    var jiraSource = sp.GetRequiredService<IOptions<JiraOptions>>().Value;
    var pdfSource = sp.GetRequiredService<IOptions<PdfOptions>>().Value;
    var csvSource = sp.GetRequiredService<IOptions<CsvOptions>>().Value;
    var uiSource = sp.GetRequiredService<IOptions<UiOptions>>().Value;
    var reports = ResolveReports(jiraSource.Reports);

    var settings = new AppSettings(
        new JiraBaseUrl(jiraSource.BaseUrl.ToString()),
        new JiraEmail(jiraSource.Email),
        new JiraApiToken(jiraSource.ApiToken),
        Math.Clamp(jiraSource.MaxResultsPerPage, 1, 100),
        jiraSource.RetryCount,
        reports,
        new PdfSettings(pdfSource.OpenAfterGeneration),
        new CsvSettings(csvSource.Enabled, csvSource.DisplayHeaders),
        uiSource.ReportSelectionPageSize is int pageSize
            ? new UiSettings(pageSize)
            : null);

    return Options.Create(settings);
});

builder.Services.AddHttpClient<IJiraTransport, JiraTransport>((sp, http) =>
{
    var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
    http.BaseAddress = new Uri(settings.BaseUrl.Value.TrimEnd('/') + "/", UriKind.Absolute);

    var raw = $"{settings.Email.Value}:{settings.ApiToken.Value}";
    var b64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", b64);
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddSingleton<ISerializer, SimpleJsonSerializer>();
builder.Services.AddSingleton<IJiraRetryPolicy, JiraRetryPolicy>();
builder.Services.AddTransient<IIssueMapper, IssueMapper>();
builder.Services.AddTransient<IJiraApiClient, JiraApiClient>();
builder.Services.AddTransient<IJiraLogicService, JiraLogicService>();
builder.Services.AddTransient<IJiraPresentationService, SpectreJiraPresentationService>();
builder.Services.AddTransient<ICsvReportWriter, CsvReportWriter>();
builder.Services.AddTransient<IPdfContentComposer, PdfContentComposer>();
builder.Services.AddTransient<IPdfReportFileStore, PdfReportFileStore>();
builder.Services.AddTransient<IPdfReportLauncher, PdfReportLauncher>();
builder.Services.AddTransient<IPdfReportRenderer, QuestPdfReportRenderer>();
builder.Services.AddTransient<IJiraApplication, JiraApplication>();

using var host = builder.Build();

var application = host.Services.GetRequiredService<IJiraApplication>();
await application.RunAsync(CancellationToken.None).ConfigureAwait(false);

static IReadOnlyList<ReportConfig> ResolveReports(IReadOnlyList<ReportConfigOptions> sourceReports)
{
    var reports = sourceReports
        .Where(static report =>
            !string.IsNullOrWhiteSpace(report.Name) &&
            !string.IsNullOrWhiteSpace(report.Jql) &&
            !string.IsNullOrWhiteSpace(report.PdfReportName))
        .Select(report => new ReportConfig(
            new ReportName(report.Name),
            new JqlQuery(report.Jql.Trim()),
            report.OutputFields is null ? [] : [.. report.OutputFields.Select(static field => new IssueFieldName(field))],
            report.CountFields is null ? [] : [.. report.CountFields.Select(static field => new IssueFieldName(field))],
            new PdfReportName(report.PdfReportName.Trim())))
        .ToList();

    if (reports.Count == 0)
    {
        throw new InvalidOperationException(
            "Jira:Reports exists but has no valid entries with Name, Jql and PdfReportName.");
    }

    return reports;
}
