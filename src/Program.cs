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

builder.Services.AddSingleton(sp =>
{
    var source = sp.GetRequiredService<IOptions<JiraOptions>>().Value;
    var baseUrl = GetRequiredSetting(
        "Jira Base URL",
        source.BaseUrl?.ToString());
    var email = GetRequiredSetting("Jira Email", source.Email);
    var apiToken = GetRequiredSetting("Jira API Token", source.ApiToken);
    var reports = ResolveReports(source.Reports);

    var settings = new AppSettings(
        new JiraBaseUrl(baseUrl),
        new JiraEmail(email),
        new JiraApiToken(apiToken),
        Math.Clamp(source.MaxResultsPerPage, 1, 100),
        source.RetryCount,
        reports);

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
builder.Services.AddTransient<IPdfContentComposer, PdfContentComposer>();
builder.Services.AddTransient<IPdfReportFileStore, PdfReportFileStore>();
builder.Services.AddTransient<IPdfReportRenderer, QuestPdfReportRenderer>();
builder.Services.AddTransient<IJiraApplication, JiraApplication>();

using var host = builder.Build();

var application = host.Services.GetRequiredService<IJiraApplication>();
await application.RunAsync(CancellationToken.None).ConfigureAwait(false);

static string GetRequiredSetting(string label, string? currentValue)
{
    if (HasConfiguredValue(currentValue))
    {
        return currentValue!.Trim();
    }

    throw new InvalidOperationException($"{label} is required in appsettings.json.");
}

static bool HasConfiguredValue(string? value)
{
    return !string.IsNullOrWhiteSpace(value);
}

static IReadOnlyList<ReportConfig> ResolveReports(IReadOnlyList<ReportConfigOptions>? sourceReports)
{
    if (sourceReports is null || sourceReports.Count == 0)
    {
        throw new InvalidOperationException("Jira:Reports must contain at least one report.");
    }

    var reports = sourceReports
        .Where(static report =>
            !string.IsNullOrWhiteSpace(report.Name) &&
            !string.IsNullOrWhiteSpace(report.Jql) &&
            !string.IsNullOrWhiteSpace(report.PdfReportName))
        .Select(report => new ReportConfig(
            report.Name!.Trim(),
            report.Jql!.Trim(),
            report.OutputFields is null ? [] : [.. report.OutputFields],
            report.CountFields is null ? [] : [.. report.CountFields],
            report.PdfReportName!.Trim()))
        .ToList();

    if (reports.Count == 0)
    {
        throw new InvalidOperationException(
            "Jira:Reports exists but has no valid entries with Name, Jql and PdfReportName.");
    }

    return reports;
}
