namespace JiraReport.Models;

internal sealed record OutputColumn(
    string Key,
    string Header,
    int ConsoleWidth,
    Func<JiraIssue, string> Selector);
