namespace JiraReport.Models;

/// <summary>
/// Represents one grouped summary table in report output.
/// </summary>
/// <param name="Title">Table title.</param>
/// <param name="Rows">Grouped count rows.</param>
internal sealed record CountTable(string Title, IReadOnlyList<CountRow> Rows);
