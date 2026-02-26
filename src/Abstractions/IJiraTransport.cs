namespace JiraReport.Abstractions;

internal interface IJiraTransport
{
    Task<TDto?> GetAsync<TDto>(Uri url, CancellationToken cancellationToken);
}
