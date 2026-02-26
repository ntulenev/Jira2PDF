using JiraReport.Models;
using JiraReport.Models.ValueObjects;

namespace JiraReport.Abstractions;

/// <summary>
/// Defines user interaction and report presentation operations.
/// </summary>
internal interface IJiraPresentationService
{
    /// <summary>
    /// Selects report configuration from configured entries.
    /// </summary>
    /// <param name="sourceReports">Configured reports.</param>
    /// <returns>Selected report or null.</returns>
    ReportConfig? SelectReportConfig(IReadOnlyList<ReportConfig> sourceReports);

    /// <summary>
    /// Resolves output PDF path from interactive input.
    /// </summary>
    /// <param name="defaultPdfPath">Default output path.</param>
    /// <returns>Resolved output path.</returns>
    string ResolvePdfPath(string defaultPdfPath);

    /// <summary>
    /// Displays prepared report in console.
    /// </summary>
    /// <param name="report">Report model.</param>
    /// <param name="outputColumns">Selected output columns.</param>
    void ShowReport(JiraJqlReport report, IReadOnlyList<OutputColumn> outputColumns);

    /// <summary>
    /// Displays message about saved PDF file.
    /// </summary>
    /// <param name="pdfPath">Saved PDF path.</param>
    void ShowPdfSaved(string pdfPath);

    /// <summary>
    /// Displays error message.
    /// </summary>
    /// <param name="errorMessage">Error details.</param>
    void ShowError(ErrorMessage errorMessage);

    /// <summary>
    /// Runs async action with loading status UI and returns a value.
    /// </summary>
    /// <typeparam name="T">Result type.</typeparam>
    /// <param name="title">Initial loading title.</param>
    /// <param name="action">Action that can update status text.</param>
    /// <returns>Action result.</returns>
    Task<T> RunLoadingAsync<T>(string title, Func<Action<string>, Task<T>> action);

    /// <summary>
    /// Runs async action with loading status UI.
    /// </summary>
    /// <param name="title">Initial loading title.</param>
    /// <param name="action">Action that can update status text.</param>
    /// <returns>Asynchronous operation task.</returns>
    Task RunLoadingAsync(string title, Func<Action<string>, Task> action);
}
