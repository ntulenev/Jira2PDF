namespace JiraReport.Models;

/// <summary>
/// Represents one grouped summary table in report output.
/// </summary>
internal sealed record CountTable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountTable"/> record.
    /// </summary>
    /// <param name="title">Table title.</param>
    /// <param name="rows">Grouped count rows.</param>
    public CountTable(string title, IReadOnlyList<CountRow> rows)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(rows);

        Title = title.Trim();
        Rows = rows;
    }

    /// <summary>
    /// Gets table title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets grouped count rows.
    /// </summary>
    public IReadOnlyList<CountRow> Rows { get; }
}
