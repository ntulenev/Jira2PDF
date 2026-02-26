using System.Net;

namespace JiraReport.Abstractions;

/// <summary>
/// Defines retry strategy for Jira transport requests.
/// </summary>
internal interface IJiraRetryPolicy
{
    /// <summary>
    /// Gets retry delay for request attempt.
    /// </summary>
    /// <param name="retryAttempt">1-based retry attempt number.</param>
    /// <param name="statusCode">HTTP status code when available.</param>
    /// <param name="exception">Exception when available.</param>
    /// <param name="delay">Calculated delay when retry is allowed.</param>
    /// <returns>True when operation should be retried.</returns>
    bool TryGetDelay(int retryAttempt, HttpStatusCode? statusCode, Exception? exception, out TimeSpan delay);
}
