namespace JiraReport.Models;

/// <summary>
/// Represents one row in grouped count output.
/// </summary>
/// <param name="Name">Group name.</param>
/// <param name="Count">Items count for the group.</param>
internal sealed record CountRow(string Name, int Count);
