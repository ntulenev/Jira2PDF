using System.Net;

namespace JiraReport.Abstractions;

internal interface IJiraRetryPolicy
{
    bool TryGetDelay(int retryAttempt, HttpStatusCode? statusCode, Exception? exception, out TimeSpan delay);
}
