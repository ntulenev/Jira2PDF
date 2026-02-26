namespace JiraReport.Models;

/// <summary>
/// Represents one row in grouped count output.
/// </summary>
internal sealed record CountRow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountRow"/> record.
    /// </summary>
    /// <param name="name">Group name.</param>
    /// <param name="count">Items count for the group.</param>
    public CountRow(string name, int count)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        Name = name.Trim();
        Count = count;
    }

    /// <summary>
    /// Gets group name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets items count for the group.
    /// </summary>
    public int Count { get; }
}
