namespace JiraReport.Models;

/// <summary>
/// Defines a report output column.
/// </summary>
/// <param name="Key">Column key.</param>
/// <param name="Header">Displayed header label.</param>
/// <param name="ConsoleWidth">Column width in console output.</param>
/// <param name="Selector">Value selector for an issue row.</param>
internal sealed record OutputColumn(
    string Key,
    string Header,
    int ConsoleWidth,
    Func<JiraIssue, string> Selector);
