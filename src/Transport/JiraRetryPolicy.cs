using System.Net;

using JiraReport.Abstractions;
using JiraReport.Models.Configuration;

using Microsoft.Extensions.Options;

namespace JiraReport.Transport;

internal sealed class JiraRetryPolicy : IJiraRetryPolicy
{
    public JiraRetryPolicy(IOptions<AppSettings> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _settings = options.Value;
    }

    public bool TryGetDelay(int retryAttempt, HttpStatusCode? statusCode, Exception? exception, out TimeSpan delay)
    {
        if (retryAttempt <= 0 || retryAttempt > _settings.RetryCount)
        {
            delay = TimeSpan.Zero;
            return false;
        }

        if (exception is HttpRequestException)
        {
            delay = TimeSpan.FromMilliseconds(BASE_DELAY_MS * retryAttempt);
            return true;
        }

        if (statusCode is not null && IsRetryable(statusCode.Value))
        {
            delay = TimeSpan.FromMilliseconds(BASE_DELAY_MS * retryAttempt);
            return true;
        }

        delay = TimeSpan.Zero;
        return false;
    }

    private static bool IsRetryable(HttpStatusCode statusCode)
    {
        var code = (int)statusCode;
        return statusCode == HttpStatusCode.TooManyRequests || code >= 500;
    }

    private const int BASE_DELAY_MS = 200;
    private readonly AppSettings _settings;
}
