namespace JiraReport.Models.ValueObjects;

/// <summary>
/// Represents validated report configuration name value.
/// </summary>
internal readonly record struct ReportName
{
    /// <summary>
    /// Initializes a new <see cref="ReportName"/> instance.
    /// </summary>
    /// <param name="value">Raw report configuration name value.</param>
    public ReportName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    /// <summary>
    /// Gets normalized report configuration name text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Returns report configuration name text representation.
    /// </summary>
    /// <returns>Report configuration name text.</returns>
    public override string ToString() => Value;
}
