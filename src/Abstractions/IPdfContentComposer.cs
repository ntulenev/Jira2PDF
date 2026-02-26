using JiraReport.Models;
using JiraReport.Models.ValueObjects;

using QuestPDF.Fluent;

namespace JiraReport.Abstractions;

/// <summary>
/// Defines PDF content composition for the report document.
/// </summary>
internal interface IPdfContentComposer
{
    /// <summary>
    /// Composes report sections into the PDF content column.
    /// </summary>
    /// <param name="column">Target PDF column descriptor.</param>
    /// <param name="report">Prepared report model.</param>
    /// <param name="outputColumns">Selected output columns.</param>
    /// <param name="baseUrl">Jira base URL for issue links.</param>
    void ComposeContent(
        ColumnDescriptor column,
        JiraJqlReport report,
        IReadOnlyList<OutputColumn> outputColumns,
        JiraBaseUrl baseUrl);
}
