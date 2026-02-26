using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

Console.OutputEncoding = Encoding.UTF8;

try
{
    var appSettings = AppSettingsLoader.Load();
    var jiraSettings = ResolveRuntimeJiraSettings(appSettings.Jira);
    var selectedConfig = ResolveSelectedReportConfig(appSettings.Jira.Reports);

    var jql = selectedConfig is null ? ResolveJql(args) : selectedConfig.Jql.Trim();
    var outputColumns = OutputColumnCatalog.Resolve(selectedConfig?.OutputFields);
    var reportTitle = ResolveReportTitle(selectedConfig);

    var defaultPdfPath = BuildDefaultPdfPath(jiraSettings.DefaultPdfPath, reportTitle);
    var pdfPath = ResolvePdfPath(defaultPdfPath);

    using var httpClient = CreateHttpClient(jiraSettings);
    var jiraClient = new JiraSearchClient(httpClient);

    var issues = await jiraClient
        .SearchIssuesAsync(jql, jiraSettings.MaxResultsPerPage, CancellationToken.None)
        .ConfigureAwait(false);

    var report = JqlReport.Create(reportTitle, selectedConfig?.Name, jql, issues);

    ConsoleReportWriter.Write(report, outputColumns);
    PdfReportWriter.Write(report, jiraSettings.BaseUrl, pdfPath, outputColumns);

    Console.WriteLine();
    Console.WriteLine($"PDF report saved to: {pdfPath}");
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine($"Failed to generate Jira report: {ex.Message}");
    return 1;
}

static RuntimeJiraSettings ResolveRuntimeJiraSettings(JiraSettings source)
{
    ArgumentNullException.ThrowIfNull(source);

    var baseUrl = PromptRequired(
        "Jira Base URL",
        source.BaseUrl,
        "https://your-company.atlassian.net");
    var email = PromptRequired("Jira Email", source.Email, "your-email@company.com");
    var apiToken = PromptRequired("Jira API Token", source.ApiToken, "your-jira-api-token", true);

    var normalizedBaseUrl = NormalizeBaseUrl(baseUrl);
    var pageSize = Math.Clamp(source.MaxResultsPerPage, 1, 100);
    var defaultPdfPath = string.IsNullOrWhiteSpace(source.DefaultPdfPath)
        ? "jql-report.pdf"
        : source.DefaultPdfPath.Trim();

    return new RuntimeJiraSettings(
        normalizedBaseUrl,
        email.Trim(),
        apiToken.Trim(),
        pageSize,
        defaultPdfPath);
}

static string ResolveJql(IReadOnlyList<string> args)
{
    if (args.Count > 0)
    {
        var joined = string.Join(" ", args).Trim();
        if (!string.IsNullOrWhiteSpace(joined))
        {
            return joined;
        }
    }

    while (true)
    {
        Console.Write("Enter JQL: ");
        var input = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(input))
        {
            return input.Trim();
        }

        Console.WriteLine("JQL cannot be empty.");
    }
}

static ReportConfig? ResolveSelectedReportConfig(IReadOnlyList<ReportConfig>? sourceConfigs)
{
    if (sourceConfigs is null || sourceConfigs.Count == 0)
    {
        return null;
    }

    var configs = sourceConfigs
        .Where(static config => !string.IsNullOrWhiteSpace(config.Name) && !string.IsNullOrWhiteSpace(config.Jql))
        .ToList();

    if (configs.Count == 0)
    {
        throw new InvalidOperationException("Jira:Reports exists but has no valid entries with Name and Jql.");
    }

    Console.WriteLine();
    Console.WriteLine("Select report config:");
    for (var i = 0; i < configs.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {configs[i].Name}");
    }

    while (true)
    {
        Console.Write("Config number: ");
        var input = Console.ReadLine();
        if (int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number)
            && number >= 1
            && number <= configs.Count)
        {
            return configs[number - 1];
        }

        Console.WriteLine($"Please enter a number between 1 and {configs.Count}.");
    }
}

static string ResolveReportTitle(ReportConfig? selectedConfig)
{
    if (selectedConfig is null)
    {
        return "Jira JQL Report";
    }

    if (!string.IsNullOrWhiteSpace(selectedConfig.PdfReportName))
    {
        return selectedConfig.PdfReportName.Trim();
    }

    return selectedConfig.Name.Trim();
}

static string ResolvePdfPath(string defaultPdfPath)
{
    Console.Write($"PDF output path [{defaultPdfPath}]: ");
    var input = Console.ReadLine();
    var selectedPath = string.IsNullOrWhiteSpace(input)
        ? defaultPdfPath
        : input.Trim();

    if (!selectedPath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
    {
        selectedPath += ".pdf";
    }

    selectedPath = Path.GetFullPath(selectedPath);

    var directory = Path.GetDirectoryName(selectedPath);
    if (!string.IsNullOrWhiteSpace(directory))
    {
        Directory.CreateDirectory(directory);
    }

    return selectedPath;
}

static string BuildDefaultPdfPath(string configuredPath, string reportTitle)
{
    var basePath = string.IsNullOrWhiteSpace(configuredPath)
        ? "jql-report.pdf"
        : configuredPath.Trim();

    if (!basePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
    {
        basePath += ".pdf";
    }

    var directory = Path.GetDirectoryName(basePath);
    var extension = Path.GetExtension(basePath);
    var fallbackFileName = Path.GetFileNameWithoutExtension(basePath);
    var fileName = string.IsNullOrWhiteSpace(reportTitle)
        ? fallbackFileName
        : SanitizeFileName(reportTitle);
    if (string.IsNullOrWhiteSpace(fileName))
    {
        fileName = fallbackFileName;
    }

    var timestampedFileName = $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";

    return string.IsNullOrWhiteSpace(directory)
        ? Path.GetFullPath(timestampedFileName)
        : Path.GetFullPath(Path.Combine(directory, timestampedFileName));
}

static string SanitizeFileName(string value)
{
    var invalidChars = Path.GetInvalidFileNameChars();
    var sanitized = new string(value
        .Trim()
        .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
        .ToArray());

    return string.IsNullOrWhiteSpace(sanitized)
        ? string.Empty
        : sanitized.Replace(' ', '_');
}

static string PromptRequired(string label, string? currentValue, string placeholder, bool secret = false)
{
    if (HasConfiguredValue(currentValue, placeholder))
    {
        return currentValue!.Trim();
    }

    while (true)
    {
        Console.Write($"{label}: ");
        var input = secret ? ReadSecret() : Console.ReadLine() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(input))
        {
            return input.Trim();
        }

        Console.WriteLine($"{label} cannot be empty.");
    }
}

static bool HasConfiguredValue(string? value, string placeholder)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    var trimmed = value.Trim();
    if (trimmed.Equals(placeholder, StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    if (trimmed.Contains("your-", StringComparison.OrdinalIgnoreCase))
    {
        return false;
    }

    return true;
}

static string ReadSecret()
{
    var buffer = new StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (buffer.Length > 0)
            {
                buffer.Length--;
            }

            continue;
        }

        if (!char.IsControl(key.KeyChar))
        {
            buffer.Append(key.KeyChar);
        }
    }

    return buffer.ToString();
}

static string NormalizeBaseUrl(string rawBaseUrl)
{
    if (!Uri.TryCreate(rawBaseUrl, UriKind.Absolute, out var uri))
    {
        throw new InvalidOperationException($"Invalid Jira Base URL '{rawBaseUrl}'.");
    }

    if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
        && !string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Jira Base URL must start with http:// or https://.");
    }

    return uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
}

static HttpClient CreateHttpClient(RuntimeJiraSettings settings)
{
    var client = new HttpClient
    {
        BaseAddress = new Uri(settings.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute),
        Timeout = TimeSpan.FromSeconds(100)
    };

    var rawAuth = $"{settings.Email}:{settings.ApiToken}";
    var encodedAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes(rawAuth));

    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedAuth);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    return client;
}

internal sealed record RuntimeJiraSettings(
    string BaseUrl,
    string Email,
    string ApiToken,
    int MaxResultsPerPage,
    string DefaultPdfPath);

internal sealed class AppSettings
{
    public JiraSettings Jira { get; set; } = new();
}

internal sealed class JiraSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ApiToken { get; set; } = string.Empty;
    public int MaxResultsPerPage { get; set; } = 100;
    public string DefaultPdfPath { get; set; } = "jql-report.pdf";
    public List<ReportConfig> Reports { get; set; } = [];
}

internal sealed class ReportConfig
{
    public string Name { get; set; } = string.Empty;
    public string Jql { get; set; } = string.Empty;
    public List<string> OutputFields { get; set; } = [];
    public string PdfReportName { get; set; } = string.Empty;
}

internal static class AppSettingsLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static AppSettings Load()
    {
        var currentDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        var outputDirectoryPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var configPath = File.Exists(currentDirectoryPath) ? currentDirectoryPath : outputDirectoryPath;

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException(
                "appsettings.json not found. Create it in the project root or output folder.");
        }

        var rawJson = File.ReadAllText(configPath);
        var appSettings = JsonSerializer.Deserialize<AppSettings>(rawJson, SerializerOptions);

        return appSettings ?? new AppSettings();
    }
}

internal static class OutputColumnCatalog
{
    private static readonly IReadOnlyDictionary<string, OutputColumn> Columns =
        new Dictionary<string, OutputColumn>(StringComparer.OrdinalIgnoreCase)
        {
            ["key"] = new OutputColumn("key", "Key", 12, static issue => issue.Key),
            ["summary"] = new OutputColumn("summary", "Summary", 52, static issue => issue.Summary),
            ["status"] = new OutputColumn("status", "Status", 18, static issue => issue.Status),
            ["issuetype"] = new OutputColumn("issuetype", "Type", 14, static issue => issue.IssueType),
            ["assignee"] = new OutputColumn("assignee", "Assignee", 24, static issue => issue.Assignee),
            ["created"] = new OutputColumn(
                "created",
                "Created",
                10,
                static issue => issue.Created.HasValue ? issue.Created.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "-"),
            ["updated"] = new OutputColumn(
                "updated",
                "Updated",
                10,
                static issue => issue.Updated.HasValue ? issue.Updated.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : "-")
        };

    private static readonly IReadOnlyList<string> DefaultOrder =
        ["key", "issuetype", "status", "assignee", "created", "updated", "summary"];

    public static IReadOnlyList<OutputColumn> Resolve(IReadOnlyList<string>? configuredFields)
    {
        var fields = configuredFields is { Count: > 0 } ? configuredFields : DefaultOrder;
        var resolved = new List<OutputColumn>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawField in fields)
        {
            if (string.IsNullOrWhiteSpace(rawField))
            {
                continue;
            }

            var normalizedField = rawField.Trim();
            if (!Columns.TryGetValue(normalizedField, out var column))
            {
                throw new InvalidOperationException(
                    $"Unsupported output field '{normalizedField}'. Supported values: {string.Join(", ", Columns.Keys)}.");
            }

            if (seen.Add(column.Key))
            {
                resolved.Add(column);
            }
        }

        if (resolved.Count > 0)
        {
            return resolved;
        }

        return [.. DefaultOrder.Select(static field => Columns[field])];
    }
}

internal sealed record OutputColumn(string Key, string Header, int ConsoleWidth, Func<JiraIssue, string> Selector);

internal sealed class JiraSearchClient
{
    private const string IssueFields = "key,summary,status,issuetype,assignee,created,updated";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public JiraSearchClient(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;
    }

    public async Task<IReadOnlyList<JiraIssue>> SearchIssuesAsync(
        string jql,
        int maxResultsPerPage,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jql);
        var pageSize = Math.Clamp(maxResultsPerPage, 1, 100);

        try
        {
            return await SearchWithPageTokenAsync(jql, pageSize, cancellationToken).ConfigureAwait(false);
        }
        catch (JiraApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return await SearchWithStartAtAsync(jql, pageSize, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task<IReadOnlyList<JiraIssue>> SearchWithPageTokenAsync(
        string jql,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var issues = new List<JiraIssue>();
        string? nextPageToken = null;

        while (true)
        {
            var url = $"rest/api/3/search/jql?jql={Uri.EscapeDataString(jql)}&fields={IssueFields}&maxResults={pageSize}";
            if (!string.IsNullOrWhiteSpace(nextPageToken))
            {
                url += $"&nextPageToken={Uri.EscapeDataString(nextPageToken)}";
            }

            var page = await GetSearchPageAsync(url, cancellationToken).ConfigureAwait(false);
            issues.AddRange(MapIssues(page));

            nextPageToken = page.NextPageToken;
            if (page.Issues.Count == 0 || page.IsLast || string.IsNullOrWhiteSpace(nextPageToken))
            {
                break;
            }
        }

        return [.. issues
            .DistinctBy(static issue => issue.Key, StringComparer.OrdinalIgnoreCase)
            .OrderBy(static issue => issue.Key, StringComparer.OrdinalIgnoreCase)];
    }

    private async Task<IReadOnlyList<JiraIssue>> SearchWithStartAtAsync(
        string jql,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var issues = new List<JiraIssue>();
        var startAt = 0;

        while (true)
        {
            var url =
                $"rest/api/3/search?jql={Uri.EscapeDataString(jql)}&fields={IssueFields}&startAt={startAt}&maxResults={pageSize}";
            var page = await GetSearchPageAsync(url, cancellationToken).ConfigureAwait(false);
            issues.AddRange(MapIssues(page));

            if (page.Issues.Count == 0)
            {
                break;
            }

            startAt += page.Issues.Count;
            var total = page.Total > 0 ? page.Total : startAt;
            if (startAt >= total)
            {
                break;
            }
        }

        return [.. issues
            .DistinctBy(static issue => issue.Key, StringComparer.OrdinalIgnoreCase)
            .OrderBy(static issue => issue.Key, StringComparer.OrdinalIgnoreCase)];
    }

    private async Task<JiraSearchResponse> GetSearchPageAsync(string url, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new JiraApiException(
                response.StatusCode,
                $"Jira API request failed ({(int)response.StatusCode} {response.ReasonPhrase}). Url='{url}'. Body='{body}'.");
        }

        var page = JsonSerializer.Deserialize<JiraSearchResponse>(body, SerializerOptions);
        return page ?? throw new InvalidOperationException("Jira search response is empty.");
    }

    private static IReadOnlyList<JiraIssue> MapIssues(JiraSearchResponse page)
    {
        if (page.Issues.Count == 0)
        {
            return [];
        }

        return [.. page.Issues
            .Where(static rawIssue => !string.IsNullOrWhiteSpace(rawIssue.Key))
            .Select(static rawIssue =>
            {
                var fields = rawIssue.Fields;
                return new JiraIssue(
                    rawIssue.Key!.Trim(),
                    string.IsNullOrWhiteSpace(fields?.Summary) ? "(no summary)" : fields.Summary.Trim(),
                    string.IsNullOrWhiteSpace(fields?.Status?.Name) ? "Unknown" : fields.Status.Name.Trim(),
                    string.IsNullOrWhiteSpace(fields?.IssueType?.Name) ? "Unknown" : fields.IssueType.Name.Trim(),
                    string.IsNullOrWhiteSpace(fields?.Assignee?.DisplayName) ? "Unassigned" : fields.Assignee.DisplayName.Trim(),
                    ParseDateTimeOffset(fields?.Created),
                    ParseDateTimeOffset(fields?.Updated));
            })];
    }

    private static DateTimeOffset? ParseDateTimeOffset(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }
}

internal sealed class JiraApiException : Exception
{
    public JiraApiException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public HttpStatusCode StatusCode { get; }
}

internal sealed class JiraSearchResponse
{
    [JsonPropertyName("issues")]
    public List<JiraIssueResponse> Issues { get; set; } = [];

    [JsonPropertyName("isLast")]
    public bool IsLast { get; set; }

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

internal sealed class JiraIssueResponse
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("fields")]
    public JiraIssueFieldsResponse? Fields { get; set; }
}

internal sealed class JiraIssueFieldsResponse
{
    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    [JsonPropertyName("status")]
    public JiraNamedEntityResponse? Status { get; set; }

    [JsonPropertyName("issuetype")]
    public JiraNamedEntityResponse? IssueType { get; set; }

    [JsonPropertyName("assignee")]
    public JiraAssigneeResponse? Assignee { get; set; }

    [JsonPropertyName("created")]
    public string? Created { get; set; }

    [JsonPropertyName("updated")]
    public string? Updated { get; set; }
}

internal sealed class JiraNamedEntityResponse
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

internal sealed class JiraAssigneeResponse
{
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }
}

internal sealed record JiraIssue(
    string Key,
    string Summary,
    string Status,
    string IssueType,
    string Assignee,
    DateTimeOffset? Created,
    DateTimeOffset? Updated);

internal sealed record CountRow(string Name, int Count);

internal sealed record JqlReport(
    string Title,
    string? ConfigName,
    string Jql,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<JiraIssue> Issues,
    IReadOnlyList<CountRow> ByStatus,
    IReadOnlyList<CountRow> ByIssueType,
    IReadOnlyList<CountRow> ByAssignee)
{
    public static JqlReport Create(string title, string? configName, string jql, IReadOnlyList<JiraIssue> issues)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(jql);
        ArgumentNullException.ThrowIfNull(issues);

        return new JqlReport(
            title.Trim(),
            string.IsNullOrWhiteSpace(configName) ? null : configName.Trim(),
            jql.Trim(),
            DateTimeOffset.Now,
            [.. issues],
            GroupByCount(issues, static issue => issue.Status),
            GroupByCount(issues, static issue => issue.IssueType),
            GroupByCount(issues, static issue => issue.Assignee));
    }

    private static IReadOnlyList<CountRow> GroupByCount(
        IReadOnlyList<JiraIssue> issues,
        Func<JiraIssue, string> selector)
    {
        return [.. issues
            .GroupBy(
                issue =>
                {
                    var value = selector(issue);
                    return string.IsNullOrWhiteSpace(value) ? "Unknown" : value.Trim();
                },
                StringComparer.OrdinalIgnoreCase)
            .Select(static group => new CountRow(group.Key, group.Count()))
            .OrderByDescending(static group => group.Count)
            .ThenBy(static group => group.Name, StringComparer.OrdinalIgnoreCase)];
    }
}

internal static class ConsoleReportWriter
{
    private const int ConsoleIssueLimit = 50;

    public static void Write(JqlReport report, IReadOnlyList<OutputColumn> outputColumns)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(outputColumns);

        Console.WriteLine();
        Console.WriteLine(report.Title.ToUpperInvariant());
        Console.WriteLine(new string('=', 100));
        Console.WriteLine($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss zzz}");
        if (!string.IsNullOrWhiteSpace(report.ConfigName))
        {
            Console.WriteLine($"Config: {report.ConfigName}");
        }

        Console.WriteLine($"JQL: {report.Jql}");
        Console.WriteLine($"Total issues: {report.Issues.Count}");
        Console.WriteLine(new string('=', 100));

        WriteCountSection("By Status", report.ByStatus);
        WriteCountSection("By Issue Type", report.ByIssueType);
        WriteCountSection("By Assignee", report.ByAssignee);
        WriteIssuesTable(report.Issues, outputColumns);
    }

    private static void WriteCountSection(string title, IReadOnlyList<CountRow> counts)
    {
        Console.WriteLine();
        Console.WriteLine(title);
        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"{Fit("Name", 36)} {Fit("Count", 10)}");
        Console.WriteLine(new string('-', 50));

        foreach (var row in counts)
        {
            Console.WriteLine($"{Fit(row.Name, 36)} {Fit(row.Count.ToString(CultureInfo.InvariantCulture), 10)}");
        }
    }

    private static void WriteIssuesTable(IReadOnlyList<JiraIssue> issues, IReadOnlyList<OutputColumn> outputColumns)
    {
        Console.WriteLine();
        Console.WriteLine("Issues");

        var totalWidth = Math.Max(outputColumns.Sum(static column => column.ConsoleWidth + 1), 50);
        Console.WriteLine(new string('-', totalWidth));
        Console.WriteLine(string.Join(" ", outputColumns.Select(static column => Fit(column.Header, column.ConsoleWidth))));
        Console.WriteLine(new string('-', totalWidth));

        var printed = 0;
        foreach (var issue in issues.Take(ConsoleIssueLimit))
        {
            Console.WriteLine(string.Join(" ", outputColumns.Select(column => Fit(column.Selector(issue), column.ConsoleWidth))));
            printed++;
        }

        if (issues.Count > printed)
        {
            Console.WriteLine(new string('-', totalWidth));
            Console.WriteLine($"Showing first {printed} issues in console. Full list is included in PDF.");
        }
    }

    private static string Fit(string value, int width)
    {
        var normalized = string.IsNullOrWhiteSpace(value)
            ? "-"
            : value.ReplaceLineEndings(" ").Trim();

        if (normalized.Length <= width)
        {
            return normalized.PadRight(width);
        }

        if (width <= 3)
        {
            return normalized[..width];
        }

        return $"{normalized[..(width - 3)]}...";
    }
}

internal static class PdfReportWriter
{
    public static void Write(
        JqlReport report,
        string baseUrl,
        string outputPath,
        IReadOnlyList<OutputColumn> outputColumns)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(outputColumns);

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        QuestPDF.Settings.License = LicenseType.Community;

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(16);
                page.DefaultTextStyle(static style => style.FontSize(9));

                page.Header().Column(column =>
                {
                    column.Spacing(2);
                    column.Item().Text(report.Title).Bold().FontSize(15);
                    column.Item().Text($"Generated: {report.GeneratedAt:yyyy-MM-dd HH:mm:ss zzz}");
                    if (!string.IsNullOrWhiteSpace(report.ConfigName))
                    {
                        column.Item().Text($"Config: {report.ConfigName}");
                    }

                    column.Item().Text($"JQL: {report.Jql}");
                    column.Item().Text($"Total issues: {report.Issues.Count}");
                    column.Item().Text($"Jira base URL: {baseUrl}");
                });

                page.Content().PaddingTop(8).Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text("Summary by Status").Bold();
                    column.Item().Element(container => ComposeCountTable(container, report.ByStatus));

                    column.Item().Text("Summary by Issue Type").Bold();
                    column.Item().Element(container => ComposeCountTable(container, report.ByIssueType));

                    column.Item().Text("Summary by Assignee").Bold();
                    column.Item().Element(container => ComposeCountTable(container, report.ByAssignee));

                    column.Item().Text("Issues").Bold();
                    column.Item().Element(container => ComposeIssuesTable(container, report.Issues, outputColumns, baseUrl));
                });

                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf(outputPath);
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
                header.Cell().Element(HeaderCell).Text("Name");
                header.Cell().Element(HeaderCell).AlignRight().Text("Count");
            });

            foreach (var row in counts)
            {
                table.Cell().Element(BodyCell).Text(row.Name);
                table.Cell().Element(BodyCell).AlignRight().Text(row.Count.ToString(CultureInfo.InvariantCulture));
            }
        });
    }

    private static void ComposeIssuesTable(
        IContainer container,
        IReadOnlyList<JiraIssue> issues,
        IReadOnlyList<OutputColumn> outputColumns,
        string baseUrl)
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
                    header.Cell().Element(HeaderCell).Text(column.Header);
                }
            });

            foreach (var issue in issues)
            {
                foreach (var column in outputColumns)
                {
                    if (string.Equals(column.Key, "key", StringComparison.OrdinalIgnoreCase))
                    {
                        var issueUrl = BuildIssueBrowseUrl(baseUrl, issue.Key);
                        table.Cell()
                            .Element(BodyCell)
                            .Hyperlink(issueUrl)
                            .DefaultTextStyle(static style => style.FontColor(Colors.Blue.Darken2).Underline())
                            .Text(issue.Key);
                    }
                    else
                    {
                        table.Cell().Element(BodyCell).Text(column.Selector(issue));
                    }
                }
            }
        });
    }

    private static string BuildIssueBrowseUrl(string baseUrl, string issueKey)
    {
        var trimmedBaseUrl = baseUrl.TrimEnd('/');
        var escapedIssueKey = Uri.EscapeDataString(issueKey.Trim());
        return $"{trimmedBaseUrl}/browse/{escapedIssueKey}";
    }

    private static IContainer HeaderCell(IContainer container)
    {
        return container
            .Border(0.5f)
            .Background(Colors.Grey.Lighten2)
            .PaddingVertical(4)
            .PaddingHorizontal(3)
            .DefaultTextStyle(static style => style.SemiBold());
    }

    private static IContainer BodyCell(IContainer container)
    {
        return container
            .Border(0.5f)
            .PaddingVertical(3)
            .PaddingHorizontal(3);
    }
}
