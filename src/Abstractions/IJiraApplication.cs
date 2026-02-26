namespace JiraReport.Abstractions;

/// <summary>
/// Defines the application workflow entry point.
/// </summary>
internal interface IJiraApplication
{
    /// <summary>
    /// Runs the report generation workflow.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Asynchronous operation task.</returns>
    Task RunAsync(CancellationToken cancellationToken = default);
}
