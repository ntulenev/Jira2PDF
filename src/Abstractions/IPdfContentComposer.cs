using JiraReport.Models;
using JiraReport.Models.ValueObjects;

using QuestPDF.Fluent;

namespace JiraReport.Abstractions;

internal interface IPdfContentComposer
{
    void ComposeContent(
        ColumnDescriptor column,
        JiraJqlReport report,
        IReadOnlyList<OutputColumn> outputColumns,
        JiraBaseUrl baseUrl);
}
