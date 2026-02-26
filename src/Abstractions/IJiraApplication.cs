namespace JiraReport.Abstractions;

internal interface IJiraApplication
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
