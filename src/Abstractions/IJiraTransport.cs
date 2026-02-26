namespace JiraReport.Abstractions;

/// <summary>
/// Defines HTTP transport abstraction for Jira API calls.
/// </summary>
internal interface IJiraTransport
{
    /// <summary>
    /// Sends GET request and deserializes response payload.
    /// </summary>
    /// <typeparam name="TDto">Response DTO type.</typeparam>
    /// <param name="url">Relative or absolute request URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Deserialized response model.</returns>
    Task<TDto?> GetAsync<TDto>(Uri url, CancellationToken cancellationToken);
}
